using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Rhinox.Utilities;
public class WebDebuggerConfig : LoadableConfigFile<WebDebuggerConfig,ConfigFileIniLoader>
{

 
    private const string _fileName = "WebDebuggerConfig.ini";
    public override string RelativeFilePath => _fileName;

    [ConfigSection("Server")]
    public string ApiURL = "http://localhost/webdebugmaster/";
    [ConfigSection("Server")]
    public string WebURL = "http://localhost:8080";

    
}
