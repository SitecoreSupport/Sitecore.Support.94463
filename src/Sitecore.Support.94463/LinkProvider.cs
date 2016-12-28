using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sitecore.Links;
using Sitecore.Sites;
using Sitecore.Diagnostics;
using Sitecore.Web;
using Sitecore.Data.Items;
using System.Reflection;
using System.IO;

namespace Sitecore.Support.Links
{
    public class LinkProvider : Sitecore.Links.LinkProvider
    {

        public class LinkBuilder: Sitecore.Links.LinkProvider.LinkBuilder
        {
            private readonly UrlOptions _options;

            public LinkBuilder(UrlOptions options) : base(options)
            {
                this._options = options;
            }

            private static System.Collections.Generic.Dictionary<LinkProvider.LinkBuilder.SiteKey, SiteInfo> _siteResolvingTable;

            private static System.Collections.Generic.List<SiteInfo> _sites;

            private static readonly object _syncRoot = new object();
            protected System.Collections.Generic.Dictionary<LinkProvider.LinkBuilder.SiteKey, SiteInfo> GetSiteResolvingTable()
            {
                System.Collections.Generic.List<SiteInfo> sites = SiteContextFactory.Sites;
                if (!object.ReferenceEquals(LinkProvider.LinkBuilder._sites, sites))
                {
                    lock (LinkProvider.LinkBuilder._syncRoot)
                    {
                        if (!object.ReferenceEquals(LinkProvider.LinkBuilder._sites, sites))
                        {
                            LinkProvider.LinkBuilder._sites = sites;
                            LinkProvider.LinkBuilder._siteResolvingTable = null;
                        }
                    }
                }
                if (LinkProvider.LinkBuilder._siteResolvingTable == null)
                {
                    lock (LinkProvider.LinkBuilder._syncRoot)
                    {
                        if (LinkProvider.LinkBuilder._siteResolvingTable == null)
                        {
                            LinkProvider.LinkBuilder._siteResolvingTable = this.BuildSiteResolvingTable(LinkProvider.LinkBuilder._sites);
                            System.Collections.Generic.Dictionary<LinkProvider.LinkBuilder.SiteKey, SiteInfo> rebuiltTable = new Dictionary<SiteKey, SiteInfo>();
                            SiteKey sk;
                            foreach (var pair in _siteResolvingTable)
                            {
                                sk = pair.Key;
                                if (sk.Path.EndsWith("/"))
                                {
                                    string Path = sk.Path.Substring(0, sk.Path.Length - 1);
                                    SiteKey newSK = new SiteKey(Path, sk.Language);
                                    rebuiltTable.Add(newSK, pair.Value);
                                }
                                else
                                    rebuiltTable.Add(pair.Key, pair.Value);
                            }
                            LinkProvider.LinkBuilder._siteResolvingTable = rebuiltTable;
                        }
                    }
                }
                return LinkProvider.LinkBuilder._siteResolvingTable;
            }

            
            protected override SiteInfo ResolveTargetSite(Item item)
            {
                SiteContext site = Context.Site;
                SiteContext siteContext = this._options.Site ?? site;
                SiteInfo result = (siteContext != null) ? siteContext.SiteInfo : null;
                if (!this._options.SiteResolving || item.Database.Name == "core")
                {
                    return result;
                }
                if (this._options.Site != null && (site == null || this._options.Site.Name != site.Name))
                {
                    return result;
                }
                if (siteContext != null && this.MatchCurrentSite(item, siteContext))
                {
                    return result;
                }
                System.Collections.Generic.Dictionary<LinkProvider.LinkBuilder.SiteKey, SiteInfo> siteResolvingTable = this.GetSiteResolvingTable();
                string path = item.Paths.FullPath.ToLowerInvariant();
                SiteInfo siteInfo = Sitecore.Links.LinkProvider.LinkBuilder.FindMatchingSite(siteResolvingTable, Sitecore.Links.LinkProvider.LinkBuilder.BuildKey(path, item.Language.ToString())) ?? Sitecore.Links.LinkProvider.LinkBuilder.FindMatchingSiteByPath(siteResolvingTable, path);
                if (siteInfo != null)
                {
                    return siteInfo;
                }
                return result;
            }
        }

        public LinkProvider()
        {
        }
        protected override Sitecore.Links.LinkProvider.LinkBuilder CreateLinkBuilder(UrlOptions options)
        {
            return new LinkProvider.LinkBuilder(options);
        }

    }
}
