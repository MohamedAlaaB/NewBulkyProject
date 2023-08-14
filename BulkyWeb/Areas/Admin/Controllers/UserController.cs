using Bulky.DataAccess.Repository.IRepository;
using Bulky.DataAccess.Repository;
using Bulky.Models;
using Bulky.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Microsoft.DotNet.Scaffolding.Shared.Messaging;
using Bulky.Utility;
using Microsoft.AspNetCore.Authorization;
using System.Data;
using Bulky.DataAccess.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;

namespace BulkyWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.Role_Admin)]
    public class UserController : Controller
    {   private readonly ApplicationDbContext _context;
        private readonly IUnitofwork _unitofwork;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private IWebHostEnvironment _hostEnvironment;
        public UserController(IUnitofwork unitofwork ,UserManager<IdentityUser> userManager,RoleManager<IdentityRole> roleManager,ApplicationDbContext context)
        {
            _unitofwork = unitofwork;
            _userManager = userManager;
            _roleManager = roleManager;
            _context = context;

        }
        public IActionResult Index()
        {
            IEnumerable<Appuser> list = _unitofwork.AppUser.GetAll();

            return View(list);
        }
      
        public IActionResult Upsert(int? id)
        {

            if (id == 0 || id == null)
            {
                //create
                var appuser = new Appuser();
                return View(appuser);
            }
            else
            {
                //update
                var appuser = _unitofwork.AppUser.Get(x => x.Id == id.ToString());
                return View(appuser);
            }

        }
       
        [HttpPost]
        public IActionResult Upsert(Appuser appuser)
        {
            if (ModelState.IsValid)
            {
                
                if ((appuser.Id)=="0"|| appuser.Id.IsNullOrEmpty())
                {
                    _unitofwork.AppUser.Add(appuser);
                }
                else
                {
                    _unitofwork.AppUser.Update(appuser);
                }

                _unitofwork.Save();
                return RedirectToAction("Index");
            }

            return View(appuser);
        }
        public IActionResult RoleManagment(string userId)
        {
            
            var allroles = _roleManager.Roles.ToList();
            var allcompanies = _unitofwork.Company.GetAll().ToList();
            
            var uservm = new UserVM
            {
                user = _unitofwork.AppUser.Get(x => x.Id == userId, includeprops: "Company"),
                roles = allroles.ConvertAll(a =>
                {
                    return new SelectListItem()
                    {
                        Text = a.ToString(),
                        Value = a.ToString()
                       
                    };
                }),
                companies= allcompanies.ConvertAll(a =>
                {
                    return new SelectListItem()
                    {
                        Text = a.Name,
                        Value = a.Id.ToString()

                    };
                })
            };

            uservm.user.Role = _userManager.GetRolesAsync(uservm.user).GetAwaiter().GetResult().FirstOrDefault();
            return View(uservm);
           
        }

        [HttpPost]
        public IActionResult RoleManagment(UserVM userVM)
        {
           
           
            var oldrole = _userManager.GetRolesAsync(userVM.user).GetAwaiter().GetResult().FirstOrDefault();
            var appuser = _unitofwork.AppUser.Get(x => x.Id == userVM.user.Id);
            if (oldrole != userVM.user.Role)
            {
               
                if (userVM.user.Role == SD.Role_Company && userVM.user.CompanyId != null)
                {
                    appuser.Role = SD.Role_Company;
                    appuser.CompanyId= userVM.user.CompanyId;
                }else if (userVM.user.Role != SD.Role_Company && userVM.user.Role !=null)
                {
                    appuser.Role = userVM.user.Role;
                }
                else if (oldrole == SD.Role_Company)
                {
                    appuser.CompanyId = null;
                }

                _unitofwork.AppUser.Update(appuser);
                _unitofwork.Save();
                _userManager.RemoveFromRoleAsync(appuser ,oldrole).GetAwaiter().GetResult();
                _userManager.AddToRoleAsync(appuser,userVM.user.Role).GetAwaiter().GetResult() ;
                
            }
            else
            {
                if (SD.Role_Company == oldrole && userVM.user.CompanyId != appuser.CompanyId)
                {
                    appuser.CompanyId = userVM.user.CompanyId;
                    _unitofwork.AppUser.Update (appuser);
                    _unitofwork.Save();
                }
            }
            return RedirectToAction("Index");

        }
       
        #region Api calls
        [HttpGet]
        public IActionResult GetAll()
        {
            List<Appuser> list = _unitofwork.AppUser.GetAll(includeprops:"Company").ToList();
           foreach (Appuser appuser in list)
            {
                if (appuser.Role.IsNullOrEmpty())
                {
                    appuser.Role = _userManager.GetRolesAsync(appuser).GetAwaiter().GetResult().FirstOrDefault();
                    if (appuser.Company == null)
                    {
                        appuser.Company = new Company()
                        {
                            Name = "null"
                        };
                        
                    }
                }
            }
          
          
           
            return Json(new { data = list });
        }

        [HttpPost]
        public IActionResult LockUnlock([FromBody]string id)
        {
            var objfromdb =_unitofwork.AppUser.Get(x => x.Id == id);
            if (objfromdb is null)
            {
                return Json(new { Success = "false", Message = "Error" });
            }
            if (objfromdb.Role.IsNullOrEmpty())
            {
                objfromdb.Role = _userManager.GetRolesAsync(objfromdb).GetAwaiter().GetResult().FirstOrDefault();
            }
            
            if (objfromdb.LockoutEnd != null&& objfromdb.LockoutEnd > DateTime.Now)
            {
                objfromdb.LockoutEnd = DateTime.Now;
                
            }
            else
            {
                objfromdb.LockoutEnd = DateTime.Now.AddYears(100);
            }

            _unitofwork.AppUser.Update(objfromdb);
            _unitofwork.Save();
            return Json(new { Success = "true", Message = "operation successeful" });

        }

       
    } 


    #endregion
}

