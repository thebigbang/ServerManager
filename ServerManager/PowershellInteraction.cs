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
        /// <summary>
        /// Use the arp command to get the ip address of selected macAddress if possible, return an empty string else.
        /// </summary>
        /// <param name="macAddress"></param>
        /// <returns></returns>
        public static string GetIpFromMacAddr(string macAddress)
        {
            Collection<PSObject>res= Invoke("arp -a | select-string \"" + macAddress +
                       "\" |% { $_.ToString().Trim().Split(\" \")[0] }");
            if(res.Count>0)
            {
                return res.First().BaseObject.ToString();
            }
            return "";
        }
        /// <summary>
        /// Basically call Invoke(ping ipaddr);
        /// </summary>
        /// <param name="ipAddress"></param>
        /// <returns></returns>
        public static Collection<PSObject> Ping(string ipAddress)
        {
            return Invoke("ping " + ipAddress);
        }
        public static string RunElevated(string function)
        {
            return function+"\n\rfunction Run-Elevated ($scriptblock)\n\r" +
            "{\n\r" +
            "$sh = new-object -com 'Shell.Application'\n\r" +
            "$sh.ShellExecute('powershell', \"-NoExit -Command $sb\", '', 'runas')\n\r" +
            "}\n\r" +
            "Run-Elevated(Code)\n\r";
        }
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