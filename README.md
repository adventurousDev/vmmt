## VM Management Tool

### Build

The easiest way to build the project is via Visual Studio. 

The "Setup" project requires the following Visual Studio extension: https://marketplace.visualstudio.com/items?itemName=VisualStudioClient.MicrosoftVisualStudio2017InstallerProjects

### Configuration

The application is initially packaged with:

* A hardcoded URL pointing to the configuration file: config.xml. (Currently it is https://www.dropbox.com/s/ukiu481to4i8ndn/config.xml?dl=1)
* Default versions of: config.xml and <OSOT configuration template>.xml as a fallback in case these files are not reachable over the network or in some way corrupted
  
*config.xml* has:

* URL for update manifest JSON: update.json
* URL for downloading OSOT the template
* List of external tools that the app needs(like SDelete): with URL and instructions for unzipping if needed

```XML
<!-- remote -->
<configuration>	
	<updateManifestURL>https://www.dropbox.com/s/0kbyj4guwn3qpbr/update.json?raw=1</updateManifestURL>
	<osotTemplateURL>https://www.dropbox.com/s/ig7g98lsdw991gi/bwlp_Windows_10.xml?dl=1</osotTemplateURL>
	<externalTools isList="true">
		<tool>
			<name>SDelete</name>
			<isZipped>true</isZipped>
			<!-- if true, will always download and replace the exisitng files -->
			<forceUpdate>false</forceUpdate>
			<!-- Files to extract OR check for before attempting downlaod. CSV. -->
			<files>sdelete.exe,sdelete64.exe,Eula.txt</files>
			<URL>https://download.sysinternals.com/files/SDelete.zip</URL>
		</tool>
		<tool>
			<name>PsExec</name>
			<isZipped>true</isZipped>			
			<forceUpdate>false</forceUpdate>
			<files>PsExec.exe,PsExec64.exe,Eula.txt</files>
			<URL>https://download.sysinternals.com/files/PSTools.zip</URL>
		</tool>		
	</externalTools>	
	<helpURL>https://www.bwlehrpool.de/doku.php/client/vm_anpassen</helpURL>
</configuration>
```

*update.json* has:

* The latest available version
* The download URL

```JSON
{
	"version":"1.0.1",
	"URL":"https://www.dropbox.com/s/yvwvyejltlmsqpi/setup.zip?dl=1"
}
```
### Workflow

Install:

* The app is installed via setup.exe, which requires elevation to run.
* If missing it downloads and installs the used .NET Framework 4.5
* There is a checkbox at the end which allows to start the app after install
* The app is installed in "C:\Program Files (x86)\bwLehrpool\VM Management Tool\"

Initialization:

On each startup

* The config.xml is updated from the set URL
* Updates availability is checked and in case there are any user gets notified
* The OSOT xml is updated from the set URL
* If not yet present, the external tools are downloaded and deployed

Debug:

* In Tools > Options > Log, the detail level for logs cab be chosen. For verbose output choose Debug (by default for testing period)
* The log file, log.log can be found in install directory and also exported via Options > Log > Export

User workflows:

* The workflow is divided in 3 modules: Windows Updates, System Optimizations, Disk Cleanup(cleanmgr, sdelete, defrag, dism)
* The app starts at the home "Welcome" page where a short description and instructions are presented
* From home page, user can choose between "Basic" and "Advanced" run modes
  * "Basic" starts the optimization steps right away and performs task according to default(currently hardcoded) parameters
  * "Advanced" leads to a Configuration page, where user can choose which modules to run and with what parameters
* The optimizations are then ran without any user interaction(unless explicitly chosen): this includes PC reboots if these are required(for example during Win Updates)
* At the end, a report is presented with the statuses and any errors

## License
See the [LICENSE](LICENSE.md) file for license rights and limitations (MIT).
