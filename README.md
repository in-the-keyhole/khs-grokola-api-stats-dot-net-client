# khs-grokola-api-stats-dot-net-client

.Net filter that will send API call statistics to Grokola.

Installation
____________
Pull down the dll and add it as a Reference to your .Net application.  Inside the Reference Manager, point the path to where this dll is stored on your machine.

Configure the Api Stats Filter through your Web.config:

    <add key="service-name" value="Grokola" />
    <add key="api-pattern" value="/sherpa/.*" />
    <add key="grokola-server" value="https://beta.grokola.com" />
    <add key="token" value="3a4f66ac-4793-4c1f-9c6d-4c5ee1afd0f1" />    
    <add key="watch-threshold" value="1" />    
    <add key="reference-id" value="279" />
    <add key="sleep" value="5000"/>
    
For configuration for IIS 7 and above running in Integrated Mode, add this entry to initialize the HttpModule.  Keep in mind the type attribute is pertinent to this project's namespace and class.
    
    <system.webServer>
        <modules>      
            <add name="ApiFilter" type="Api-Stats-Client.ApiFilter, ApiFilter" />
        </modules>
    </system.webServer>  

Configuration Options
_____________________
Api Stats watch configuration options are described below.

`api-pattern` - Regular expression pattern to apply to URL route. Matching patterns will send API stats to GrokOla.

`service-name` - Name of service or application using an `API`. If not specified, server machine name will be used.

`grokola-server` - Server address of GrokOla instance where API stats will be sent.

`reference-id` - The id or the GrokOla Wiki Reference. This can be obtained from your GrokOla instance.

`token` - GrokOla integration token, available from the GrokOla Admin page.

`watch-threshold` - <OPTIONAL>, number of api calls before being published to GrokOla server. Default is 50. 

`sleep` - <OPTIONAL>, amount of time in milliseconds the filter will sleep before emitting API stats to server. Default is 10 seconds.
