using Microsoft.AspNet.Identity.Owin;
using Queue.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace Queue.Controllers
{
    [Authorize]
    public class RoleController : Controller
    {
        private ApplicationRoleManager _roleManager;

        public RoleController()
        {
        }

        public RoleController(ApplicationRoleManager roleManager)
        {
            RoleManager = roleManager;
        }

        public ApplicationRoleManager RoleManager
        {
            get
            {
                return _roleManager ?? HttpContext.GetOwinContext().Get<ApplicationRoleManager>();
            }
            private set
            {
                _roleManager = value;
            }
        }
        // GET: Role
        public ActionResult Index()
        {
            List<RoleViewModel> list = new List<RoleViewModel>();
            foreach (var role in RoleManager.Roles)
                list.Add(new RoleViewModel(role));

            return View(list);
        }

        public ActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(RoleViewModel model)
        {
            if (string.IsNullOrWhiteSpace(model.Name))
            {
                ModelState.AddModelError("Name", "Ingresa el nombre del rol.");
                return View(model);
            }
            var RoleExist = false;
            var role = new ApplicationRole() { Name = model.Name };
            foreach (var _role in RoleManager.Roles)
                if (_role.Name == model.Name)
                {
                    RoleExist = true;
                }

            if (!RoleExist)
            {
                var result = await RoleManager.CreateAsync(role);
                if (!result.Succeeded)
                {
                    ModelState.AddModelError("", "No se pudo crear el rol. Revisa el nombre e inténtalo de nuevo.");
                    return View(model);
                }
                return RedirectToAction("Index");
            }
            else
            {
                ModelState.AddModelError("Name", "Ya existe un rol con ese nombre.");
                return View(model);
            }
                
            
        }

        public async Task<ActionResult> Edit(string Id)
        {
            var role = await RoleManager.FindByIdAsync(Id);
            if (role == null) return HttpNotFound();
            return View(new RoleViewModel(role));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult>Edit(RoleViewModel model)
        {
            /*ApplicationRole role = await*/
            var role = await RoleManager.FindByIdAsync(model.Id);
            if (role == null) return HttpNotFound();
            if (string.IsNullOrWhiteSpace(model.Name))
            {
                ModelState.AddModelError("Name", "Ingresa el nombre del rol.");
                return View(model);
            }
            Boolean RoleExist = false;
            foreach (var _role in RoleManager.Roles)
                if (_role.Name == model.Name && _role.Id != model.Id)
                {
                    RoleExist = true;
                }
            if (!RoleExist)
            {
                role.Name = model.Name;
                var result = await RoleManager.UpdateAsync(role);
                if (!result.Succeeded)
                {
                    ModelState.AddModelError("", "No se pudo actualizar el rol. Revisa el nombre e inténtalo de nuevo.");
                    return View(model);
                }
                return RedirectToAction("Index");
            }
            else
            {
                ModelState.AddModelError("Name", "Ya existe un rol con ese nombre.");
                return View(model);
            }
        }

        public async Task<ActionResult> Details(string Id)
        {
            var role = await RoleManager.FindByIdAsync(Id);
            if (role == null) return HttpNotFound();
            return View(new RoleViewModel(role));
        }
        public async Task<ActionResult> Delete(string Id)
        {
            var role = await RoleManager.FindByIdAsync(Id);
            if (role == null) return HttpNotFound();
            return View(new RoleViewModel(role));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(string Id)
        {
            var role = await RoleManager.FindByIdAsync(Id);
            if (role == null) return HttpNotFound();
            var result = await RoleManager.DeleteAsync(role);
            if (!result.Succeeded)
            {
                ModelState.AddModelError("", "No se pudo eliminar el rol. Comprueba sus asociaciones e inténtalo de nuevo.");
                return View("Delete", new RoleViewModel(role));
            }
            return RedirectToAction("Index");
        }



    }
}
