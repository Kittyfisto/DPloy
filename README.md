# DPloy
Tool to distribute software to remote computers.

# A warning

**Connections between Distributor.exe and Node.exe are NOT secured IN ANY WAY. You SHOULD NOT use this over an insecure network and especially NOT over the internet.** This will be fixed with https://github.com/Kittyfisto/DPloy/issues/4, but that time has not come yet.

# Why?

You want to regularly deploy software (for example your local build of a software you're working on) to one or more remote machines and are sick and tired of performing those steps manually.

# Quick Start
Deployment script 'MyDeployment.cs':
```
public class MyDeployment
{
    public void Deploy(INode node)
    {
        node.CopyFile("Path on my local machine", "Path on the remote machine");
    }
}
```

On the remote machine (192.0.2.0), run: `> Node.exe -w <LocalComputerName>`
For as long as Node.exe is running, it will accept incoming connections from computers named 'LocalComputerName'.

On your local computer (Computer name: LocalComputerName), run: `> Distributor.exe deploy MyDeployment.cs 192.0.2.0`
This will yield an output similar to the following:

```
Loading script 'C:\Users\Simon\Documents\MyDeployment.cs'               [  OK  ] 
Compiling script 'C:\Users\Simon\Documents\MyDeployment.cs'             [  OK  ] 
Connecting to '192.0.2.0'                                               [  OK  ] 
  Copying 'Path on my local machine' to 'Path on the remote machine'    [  OK  ] 
Disconnecting from '192.0.2.0:41312'                                    [  OK  ] 
Disconnecting from ''                                                   [  OK  ] 
```
