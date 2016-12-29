# Sitecore.Support.94463
There is an issue related to site resolving in case rootPath+startItem ends with "/". In case of multisite solutions and cross-site links the link from one site to another are generated incorrectly. As a host name link contains current site (not target). Also the url contains full path to the item: myCurrntSite/sitecore/content/targetItem. 

## License  
This patch is licensed under the [Sitecore Corporation A/S License for GitHub](https://github.com/sitecoresupport/Sitecore.Support.94463/blob/master/LICENSE).  

## Download  
Downloads are available via [GitHub Releases](https://github.com/sitecoresupport/Sitecore.Support.94463/releases).  
