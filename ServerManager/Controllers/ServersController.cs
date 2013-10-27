/*This file is part of ServerManager.
 * 
 * ServerManager is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * any later version.
 *
 * CustomPages is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with Foobar.  If not, see <http://www.gnu.org/licenses/>.
 *  
 * Copyright (c) Meï-Garino Jérémy 
*/
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Management.Automation;
using System.Web;
using System.Web.Mvc;
using ServerManager.Models;

namespace ServerManager.Controllers
{
    public class ServersController : Controller
    {
        public ActionResult GetStatus(int id)
        {
            bool isOnline = false;
            database db = new database();
            Servers current = db.Servers.SingleOrDefault(s => s.Id == id);
            if (current != null)
            {
                string currentIp = current.IPAddress;
                if (string.IsNullOrEmpty(currentIp))
                {
                    currentIp = PowershellInteraction.GetIpFromMacAddr(current.MacAddress);
                }
                if (!string.IsNullOrEmpty(currentIp))
                {
                    foreach (PSObject psObject in PowershellInteraction.Invoke("ping " + currentIp))
                    {
                        string bo = psObject.BaseObject.ToString().ToLowerInvariant();
                        if (bo.Contains("host unreachable") ||
                            bo.Contains("ping request could not find host"))
                        {
                            isOnline = false;
                            break;
                        }
                        isOnline = true;
                    }
                }

            }
            return PartialView("Partials/ServerStatus", new ServerStatusModel { Id = id, IsStarted = isOnline });
        }


        public ActionResult ShutdownServer(int id)
        {
            //todo: arp -a | select-string "00-24-d4-ac-1d-b1" |% { $_.ToString().Trim().Split(" ")[0] } if remote PowerShell not set in db
            Servers s = new database().Servers.SingleOrDefault(se => se.Id == id);
            if (s == null || string.IsNullOrEmpty(s.RemotePSAddress)) throw new ArgumentNullException("id", "Was null or invalid.");
            //seems we can turn it off... will then begin:
            if (string.IsNullOrEmpty(s.Username) || string.IsNullOrEmpty(s.Password))
            {
                //todo: return to a one-time credential form.
                return View("AskCredentials");
            }
            //run powershell stuff:
            string command = "function Code{\n\r" +
            "start-service winrm\n\r" +
            "winrm s winrm/config/client '@{TrustedHosts=\"" + s.RemotePSAddress + "\"}'\n\r" +
            "$Username = '" + s.Username + "'\n\r" +
            "$Password = '" + s.Password + "'\n\r" +
            "$pass = ConvertTo-SecureString -AsPlainText $Password -Force\n\r" +
            "$Cred = New-Object System.Management.Automation.PSCredential -ArgumentList $Username,$pass\n\r" +

            "$s=New-PSSession -ConnectionUri http://" + s.RemotePSAddress + " -Credential $Cred\n\r" +
            "Invoke-Command -Session $s -ScriptBlock {shutdown -s -t 40}\n\r" +
            "}\n\r" +
            "function Run-Elevated ($scriptblock)\n\r" +
            "{\n\r" +
            "$sh = new-object -com 'Shell.Application'\n\r" +
            "$sh.ShellExecute('powershell', \"-NoExit -Command $sb\", '', 'runas')\n\r" +
            "}\n\r" +
            "Run-Elevated(Code)\n\r";
            PowershellInteraction.Invoke(command);
            return RedirectToAction("Servers", "Home");
        }

        public ActionResult StartupServer(int id)
        {
            Servers s = new database().Servers.SingleOrDefault(se => se.Id == id);
            if (s == null) throw new ArgumentNullException("id", "Was null or invalid");
            //rebuild macAddr from 00-00-00 to 0x00,0x00,0x00
            string macAddr = s.MacAddress.Split('-').Aggregate("", (current, macAddPart) => current + ("0x" + macAddPart + ","));
            if (macAddr.EndsWith(",")) macAddr = macAddr.Remove(macAddr.Length - 1);
            //run powershell command to startup the server thanks to magic packets:
            string command = "function cc{Invoke-Command -ScriptBlock{$mac = [byte[]](" + macAddr + ")\n\r" +
            "$UDPclient = new-Object System.Net.Sockets.UdpClient\n\r" +
            "$UDPclient.Connect(([System.Net.IPAddress]::Broadcast),4000)\n\r" +
            "$packet = [byte[]](,0xFF * 102)\n\r" +
            "6..101 |% { $packet[$_] = $mac[($_%6)]}\n\r" +
            "\"Send:\"\n\r" +
            "$packet\n\r" +
            "$UDPclient.Send($packet, $packet.Length)}}\n\r" +
            "function Run-Elevated ($scriptblock)\n\r" +
            "{\n\r" +
            "$sh = new-object -com 'Shell.Application'\n\r" +
            "$sh.ShellExecute('powershell', \"-NoExit -Command $sb\", '', 'runas')\n\r" +
            "}\n\r" +
            "Run-Elevated(cc)\n\r";
            PowershellInteraction.Invoke(command);
            return RedirectToAction("Servers", "Home");
        }
    }
}
