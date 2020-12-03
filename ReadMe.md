# ZipContent.PutDotIO

This project is a reader plugin for [Hakan Kutluay’s ZipContent](https://github.com/hkutluay/ZipContent.Core), supporting [Put.io](https://put.io/).

ZipContent is a utility for peeking inside ZIP archives in the cloud without fully downloading them.

## Usage

```cs
using System;
using System.Threading.Tasks;

using ZipContent.Core;
using ZipContent.PutDotIO;

var uri = new Uri("https://s01-cf.put.io/zipstream/12345678.zip?oauth_token=AABBCCDDEEFFGG");
var reader = new PutIOPartialFileReader(uri);
var lister = new ZipContentLister();
var entries = await lister.GetContents(reader);
```

## Disclaimer

*ZipContent* is maintained by Hakan Kutluay. This plugin is considered community work. It is not official and may not meet original code style and data safety requirements.

*Put.io* is a commercial service. This plugin is not endorsed by Put.io, and currently does not use the official API.

This plugin accesses your files in Put.io using standard HTTPS protocol (like a browser or download manager). Authentication is handled by Put.io servers. Partial data is analyzed to provide the information you request. Your files are not transfered anywhere else nor read for any other purposes.

Licensed under ISC.
