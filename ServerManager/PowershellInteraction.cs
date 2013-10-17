using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
using System.Threading;
using System.Web;

namespace ServerManager
{
    public class PowershellInteraction
    {
        //todo: check to add cleaner elements than bulk string.
        public static Collection<PSObject> Invoke(string command)
        {
            // Powershell
            Runspace runSpace = RunspaceFactory.CreateRunspace();
            runSpace.Open();
            Pipeline pipeline = runSpace.CreatePipeline();

            Command invokeScript = new Command("Invoke-Command");
            RunspaceInvoke invoke = new RunspaceInvoke();

            // invoke-command -computername compName -scriptblock { get-process }

            ScriptBlock sb = invoke.Invoke("{" + command + "}")[0].BaseObject as ScriptBlock;
            invokeScript.Parameters.Add("scriptBlock", sb);
            //invokeScript.Parameters.Add("computername", TextBoxServer.Text);

            pipeline.Commands.Add(invokeScript);
            return pipeline.Invoke();
        }
    }
}