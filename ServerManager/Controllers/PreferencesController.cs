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
using System.Linq;
using System.Web.Mvc;
using ServerManager.Models;

namespace ServerManager.Controllers
{
    public class PreferencesController : Controller
    {
        //
        // GET: /Preferences/

        public ActionResult Index()
        {
            return View();
        }
        public ActionResult GetAllServers()
        {
            return PartialView("ServersRUD", new database().Servers.ToList());
        }
        [HttpPost]
        public ActionResult CreateServer(Servers newServer)
        {
            database db = new database();
            db.Servers.Add(newServer);
            foreach (var dbEntityValidationResult in db.GetValidationErrors())
            {
                Console.WriteLine(dbEntityValidationResult.Entry);
            }
            db.SaveChanges();
            return View("Index");
        }

        public ActionResult DeleteServer(int id)
        {
            database db=new database();
            db.Servers.Remove(db.Servers.Single(s => s.Id == id));
            db.SaveChanges();
            return RedirectToAction("Index");
        }
        [HttpPost]
        public ActionResult UpdateServers(List<Servers> servers)
        {
            database db=new database();
            foreach (Servers server in servers)
            {
                db.Servers.Single(s => s.Id == server.Id).IPAddress = server.IPAddress;
                db.Servers.Single(s => s.Id == server.Id).FriendlyName = server.FriendlyName;
                db.Servers.Single(s => s.Id == server.Id).MacAddress = server.MacAddress;
                db.Servers.Single(s => s.Id == server.Id).Password= server.Password;
                db.Servers.Single(s => s.Id == server.Id).RemotePSAddress = server.RemotePSAddress;
                db.Servers.Single(s => s.Id == server.Id).Username= server.Username;
            }
            db.SaveChanges();
            return RedirectToAction("Index");
        }
    }
}
