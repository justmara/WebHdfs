[![Build status](https://ci.appveyor.com/api/projects/status/xrd22d2lkf2o6832?svg=true)](https://ci.appveyor.com/project/justmara/webhdfs)

# WebHdfs
Simple .NET WebHDFS client library based on Microsoft.AspNet.WebApi.Client

'Simple' means:
- no security (security off mode on hdfs)
- no logging (deal with it. maybe will be added someday later)
- really stupid simple

Code partially taken from some of Microsoft Azure Hdfs library (dont remember it now, really), rewriten and cleared of all the rubbish dependencies. So now it only depends on Microsoft.AspNet.WebApi.Client wich handles all the Http Requests.

[Get it on Nuget][nuget]
```bash
PM> Install-Package WebHdfs
```
