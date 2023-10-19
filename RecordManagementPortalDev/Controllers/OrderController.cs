﻿using System.Data;
//using Stimulsoft.Report;
//using Stimulsoft.Report.Mvc;
using System.Net;
using System.Net.Mail;
using System.Text.Json;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using RecordManagementPortalDev.Data;
using RecordManagementPortalDev.Models;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.WebUtilities;
using System.Text;

namespace RecordManagementPortalDev.Controllers
{
    public class OrderController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IConfiguration _config;
        private bool loginflag;
        public class NewOrderRequests
        {
            public int Id { get; set; }
            public int OrderId { get; set; }
            public string CustomerCode { get; set; }
            public string ContactPerson { get; set; }
            public string OrderType { get; set; }
            public int CartonQty { get; set; }
            public DateTime OrderDate { get; set; }
            public string OrderStatus { get; set; }
            public string ApproverId { get; set; }
        }
        public class NewGroupedCustDept
        {
            public Customer Customer { get; set; }
            public List<NewOrderRequests> OrderList { get; set; }
            public NewOrderRequests OrderRequest { get; set; }
            public SelectList DepartmentList { get; set; }
            public SelectList PerDeptList { get; set; }
            public SelectList CustomerList { get; set; }
            public OrderDetail OrderDetail { get; set; }
            public List<OrderDetail> OrderDetailList { get; set; }
        }
        public class GroupedCustDept
        {
            public Customer Customer { get; set; }
            public List<OrderRequests> OrderList { get; set; }
            public OrderRequests OrderRequest { get; set; }
            public SelectList DepartmentList { get; set; }
            public SelectList PerDeptList { get; set; }
            public SelectList CustomerList { get; set; }
            public OrderDetail OrderDetail { get; set; }
            public List<OrderDetail> OrderDetailList { get; set; }
        }
        public class NewJob
        {
            public int Id { get; set; }
            public string OldJobNo { get; set; }            
            public Guid JobNo { get; set; }            
            public DateTime JobDate { get; set; } = DateTime.Now;            
            public string CustCode { get; set; }            
            public string DeptCode { get; set; }            
            public DateTime BillDate { get; set; }            
            public string JobLevel { get; set; }            
            public string JobType { get; set; }            
            public string ServLevel { get; set; }            
            public string CtnType { get; set; }            
            public DateTime RequestDate { get; set; }            
            public string? Remark { get; set; }            
            public string JobOrderNo { get; set; }            
            public int TotalCtn { get; set; }            
            public string Staff { get; set; }
            public string OrderFrom { get; set; }
        }
        public IdentityOptions Options
        {
            get;
            set;
        }
        public TokenOptions Tokens
        {
            get;
            set;
        } = new TokenOptions();
        public class ScanPack
        {
            public List<LogScanPack> ListScanPack { get; set; }
        }

        public class LocationHisList
        {
            public List<LocationHistory> ListLocHistory { get; set; }
        }
        public class ScanDetail
        {
            public string ScanResult { get; set; }
            public LogScanPack LogScanPack { get; set; }

            public JobsDetLoc JobsDetLoc { get; set; }
        }

        public class JobCollect
        {
            public Customer Customer { get; set; }
            public CustomerBill CustomerBill { get; set; }
            public SelectList CustomerList { get; set; }
            public SelectList DepartmentList { get; set; }
            public Job Job { get; set; }
            public List<Job> JobList { get; set; }

            public List<JobsDetLoc> JobListLoc { get; set; }

            public JobsDetLoc JobsDetLoc { get; set; }

            public OrderRequests OrderRequests { get; set; }
            public List<OrderDetail> OrderDetailList { get; set; }

            public int carton { get; set; }

            public BillRateMaster BillRate { get; set; }

        }

        public class JobCollectList
        {
            public Customer Customer { get; set; }
            public SelectList CustomerList { get; set; }
            public SelectList DepartmentList { get; set; }
            //public Job Job { get; set; }
            public List<NewJob> JobList { get; set; }            
        }

        public class RecordModel
        {
            public string Id { get; set; }
            public string DepartmentCode { get; set; }

            public string RecordNo { get; set; }

            public DateTime DestructionDate { get; set; }
        }
        public class CustomerBill
        {
            public Customer customer { get; set; }
            public BillRateMaster billratemaster { get; set; }            
        }
        public class Approve
        {
            public OrderRequests odRequests { get; set; }
            public ApprovalSetup approveset { get; set; }
            public string username { get; set; }
            public string approvername { get; set; }
            public string backupapprover { get; set; }
            public string approveremail { get; set; }
            public string backupapproveremail { get; set; }
        }
        public class Order
        {
            public OrderRequests odRequests { get; set; }
            public List<OrderDetail> listOdDetail { get; set; }
        }
        public class RecordDetails
        {
            public JobsDetLoc JobsDetail { get; set; }
            public List<RecordDetail> ListRecordDetail { get; set; }
            public SelectList CustomerList { get; set; }
            public RecordDetail rDetail { get; set; }
        }
        public OrderController(ApplicationDbContext db, UserManager<ApplicationUser> userManager, IConfiguration config, SignInManager<ApplicationUser> signInManager)
        {
            _db = db;
            _signInManager = signInManager;
            _userManager = userManager;
            _config = config;
        }

        public static string orderlist = "";
        public static string joblist = "";
        public static string Custcode = "";
        private bool checkUser()
        {
            loginflag = _signInManager.IsSignedIn(User);
            return loginflag;
        }
        //public IActionResult Index()
        //{
        //    return View();
        //}

        public async Task<IActionResult> DJobRegisterAsync(int orderid)
        {
            checkUser();
            if (loginflag == true)
            {
                var loginuser = await _userManager.GetUserAsync(User);
                JobCollect model = new JobCollect();
                var orderDb = _db.OrderRequests.Where(x => x.OrderId == orderid).FirstOrDefault();
                var orderDetail = _db.OrderDetail.Where(y => y.OrderId == orderid).ToList();
                model.OrderRequests = orderDb;
                if (orderDb != null)
                {
                    var customerDb = (from c in _db.Customers
                                      join b in _db.BillRateMaster on c.CustomerCode equals b.CustomerCode
                                      where b.CustomerCode == orderDb.CustomerCode
                                      select new
                                      {
                                          c,
                                          b
                                      }).FirstOrDefault();
                    if (customerDb != null)
                    {
                        model.Customer = customerDb.c;
                        model.BillRate = customerDb.b;
                        model.carton = orderDetail.Count();
                    }
                }
                model.CustomerList = new SelectList(_db.Customers, "CustomerCode", "CustomerCode");
                return View(model);
            }
            else
            {
                return Redirect("~/Identity/Account/Login/");
            }
        }

        public async Task<IActionResult> JobRegisterAsync()
        {
            checkUser();
            if (loginflag == true)
            {
                var loginuser = await _userManager.GetUserAsync(User);
                JobCollect model = new JobCollect();
                model.CustomerList = new SelectList(_db.Customers, "CustomerCode", "CustomerCode");
                model.DepartmentList = new SelectList(_db.Departments.Where(y => y.CustomerCode == loginuser.CustomerCode).ToList(), "DepartmentCode", "DepartmentCode");
                return View(model);
            }
            else
            {
                return Redirect("~/Identity/Account/Login/");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveJobRegister(JobCollect model)
        {
            checkUser();
            if (loginflag == true)
            {
                var loginuser = await _userManager.GetUserAsync(User);
                Job? jobdetail = (Job?)JsonSerializer.Deserialize<Job>(joblist);
                DateTime now = DateTime.Now;
                var year = now.Year;
                var month = now.Month;
                string? OldJobNo = _db.Job.Max(x => x.OldJobNo);
                string? No = OldJobNo?.Substring(8);
                string? RunningMonth = OldJobNo?.Substring(5, 2);
                int RunningNo = 1;
                if (RunningMonth is not null)
                {
                    if (month == int.Parse(RunningMonth))
                    {
                        if (No is not null)
                        {
                            RunningNo = int.Parse(No) + 1;
                        }
                    }
                }
                JobsDetLoc jd = new JobsDetLoc();
                var UpdJobNo = year + "/" + String.Format("{0:00}", month) + "-" + String.Format("{0:0000}", RunningNo);
                if (jobdetail != null)
                {
                    model.Job.OldJobNo = UpdJobNo;
                    model.Job.Person = jobdetail.Person;
                    model.Job.DeptCode = jobdetail.DeptCode;
                    model.Job.Address1 = jobdetail.Address1;
                    model.Job.Address2 = jobdetail.Address2;
                    model.Job.Address3 = jobdetail.Address3;
                    model.Job.Address4 = jobdetail.Address4;
                    model.Job.CtnType = jobdetail.CtnType;
                    model.Job.JobType = jobdetail.JobType;
                    model.Job.ServLevel = jobdetail.ServLevel;
                    model.Job.Remark = jobdetail.Remark;
                    model.Job.RequestDate = jobdetail.RequestDate;
                    model.Job.Staff = loginuser.UserCode;
                    model.Job.JobOrderNo = "";
                    model.Job.RICQty = jobdetail.RICQty;
                    model.Job.MssInvoice = jobdetail.MssInvoice;
                    model.Job.PlasticBagQty = jobdetail.PlasticBagQty;
                    model.Job.TieQty = jobdetail.TieQty;
                    model.Job.TamperSealQty = jobdetail.TamperSealQty;
                    model.Job.IndexCard = "NA";
                    model.Job.DeleteFlag = 0;
                    model.Job.JobNo = Guid.NewGuid();
                    model.Job.CustCode = jobdetail.CustCode;                    
                    if (model.Job.RMBRemark != null)
                    {
                        model.Job.RMBRemark = "";
                    }
                    int count = _db.JobsDetLoc.Count();
                    int latestid = 0;
                    if (count > 0)
                    {
                        latestid = _db.JobsDetLoc.Max(x => x.JDId);
                    }
                    //int length = model.Job.TotalCtn;
                    if (orderlist != "")
                    {
                        List<RecordModel>? OrderDetailList = (List<RecordModel>?)JsonSerializer.Deserialize<IList<RecordModel>>(orderlist);
                        var orderdetail = OrderDetailList;
                        int length = orderdetail.Count;
                        for (int i = 0; i < length; i++)
                        {
                            jd.JDId = latestid + (i + 1);
                            jd.Cartons = orderdetail[i].RecordNo;
                            jd.JobNo = UpdJobNo;
                            jd.DeptCode = orderdetail[i].DepartmentCode;
                            jd.CustCode = jobdetail.CustCode;
                            jd.JobLevel = model.Job.JobType;
                            if (model.Job.JobType == "D1")
                            {
                                jd.Status = "Deliver";
                            }
                            else if (model.Job.JobType == "D2")
                            {
                                jd.Status = "Pulled";
                            }
                            else if (model.Job.JobType == "C1")
                            {
                                jd.Status = "Collected";
                            }
                            else if (model.Job.JobType == "C2")
                            {
                                jd.Status = "Collected";
                            }
                            jd.Location = "";
                            jd.SealNo = "";
                            jd.FileNo = "";
                            jd.ProductCode = "";
                            jd.CreatedDate = DateTime.Now;
                            jd.UpdatedDate = DateTime.Now;
                            jd.Control = "";
                            jd.Description = jobdetail.Remark;
                            jd.Staff = loginuser.UserCode;
                            _db.JobsDetLoc.Add(jd);
                            _db.SaveChanges();
                        }
                    }
                }
                _db.Job.Add(model.Job);
                _db.SaveChanges();
                return RedirectToAction("JobList");
                //var job = _db.Job.Where(x => x.JobNo == model.Job.JobNo).FirstOrDefault();
                //return RedirectToAction("DetailSavedJob", new { Id = job?.Id });
            }
            else
            {
                return Redirect("~/Identity/Account/Login/");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DSaveJobRegister(JobCollect model)
        {
            checkUser();
            if (loginflag == true)
            {
                var loginuser = await _userManager.GetUserAsync(User);
                Job? jobdetail = (Job?)JsonSerializer.Deserialize<Job>(joblist);
                DateTime now = DateTime.Now;
                var year = now.Year;
                var month = now.Month;
                string? OldJobNo = _db.Job.Max(x => x.OldJobNo);
                string? No = OldJobNo?.Substring(8);
                string? RunningMonth = OldJobNo?.Substring(5, 2);
                int RunningNo = 1;
                if (RunningMonth is not null)
                {
                    if (month == int.Parse(RunningMonth))
                    {
                        if (No is not null)
                        {
                            RunningNo = int.Parse(No) + 1;
                        }
                    }
                }
                JobsDetLoc jd = new JobsDetLoc();
                var UpdJobNo = year + "/" + String.Format("{0:00}", month) + "-" + String.Format("{0:0000}", RunningNo);
                if (jobdetail != null)
                {
                    model.Job.OldJobNo = UpdJobNo;
                    model.Job.Person = jobdetail.Person;
                    model.Job.DeptCode = jobdetail.DeptCode;
                    model.Job.Address1 = jobdetail.Address1;
                    model.Job.Address2 = jobdetail.Address2;
                    model.Job.Address3 = jobdetail.Address3;
                    model.Job.Address4 = jobdetail.Address4;
                    model.Job.CtnType = jobdetail.CtnType;
                    model.Job.JobType = jobdetail.JobType;
                    model.Job.ServLevel = jobdetail.ServLevel;
                    model.Job.Remark = jobdetail.Remark;
                    model.Job.RequestDate = jobdetail.RequestDate;
                    model.Job.Staff = loginuser.UserCode;
                    model.Job.TotalCtn = jobdetail.TotalCtn;
                    model.Job.RICQty = jobdetail.RICQty;
                    model.Job.MssInvoice = jobdetail.MssInvoice;
                    model.Job.PlasticBagQty = jobdetail.PlasticBagQty;
                    model.Job.TieQty = jobdetail.TieQty;
                    model.Job.TamperSealQty = jobdetail.TamperSealQty;
                    model.Job.IndexCard = "NA";
                    model.Job.DeleteFlag = 0;
                    model.Job.JobNo = Guid.NewGuid();
                    model.Job.CustCode = jobdetail.CustCode;
                    if (model.Job.RMBRemark != null)
                    {
                        model.Job.RMBRemark = "";
                    }

                    var orderdetail = _db.OrderDetail.Where(x => x.OrderId == int.Parse(model.Job.JobOrderNo)).ToList();
                    int length = orderdetail.Count();
                    int count = _db.JobsDetLoc.Count();
                    int latestid = 0;
                    if (count > 0)
                    {
                        latestid = _db.JobsDetLoc.Max(x => x.JDId);
                    }
                    for (int i = 0; i < length; i++)
                    {
                        jd.JDId = latestid + (i + 1);
                        jd.Cartons = orderdetail[i].RecordNo;
                        jd.JobNo = UpdJobNo;
                        jd.DeptCode = orderdetail[i].DepartmentCode;
                        jd.CustCode = jobdetail.CustCode;
                        jd.JobLevel = model.Job.JobType;
                        if (model.Job.JobType == "D1")
                        {
                            jd.Status = "Deliver";
                        }
                        else if (model.Job.JobType == "D2")
                        {
                            jd.Status = "Pulled";
                        }
                        else if (model.Job.JobType == "C1")
                        {
                            jd.Status = "Collected";
                        }
                        else if (model.Job.JobType == "C2")
                        {
                            jd.Status = "Collected";
                        }
                        jd.Location = "";
                        jd.SealNo = "";
                        jd.FileNo = "";
                        jd.ProductCode = "";
                        jd.CreatedDate = DateTime.Now;
                        jd.UpdatedDate = DateTime.Now;
                        jd.Control = "";
                        jd.Description = jobdetail.Remark;
                        jd.Staff = loginuser.UserCode;
                        _db.JobsDetLoc.Add(jd);
                        _db.SaveChanges();
                    }
                }
                _db.Job.Add(model.Job);
                _db.SaveChanges();
                return RedirectToAction("NoRegisJobs");
                //var job = _db.Job.Where(x => x.JobNo == model.Job.JobNo).FirstOrDefault();
                //return RedirectToAction("DetailSavedJob", new { Id = job?.Id });
            }
            else
            {
                return Redirect("~/Identity/Account/Login/");
            }
        }

        public async Task<IActionResult> JobListAsync(int page, string keywords = "", string CustomerCode = "", string DepartmentCode = "")
        {
            checkUser();
            if (loginflag == true)
            {
                var loginuser = await _userManager.GetUserAsync(User);
                JobCollectList model = new JobCollectList();
                string userRole = loginuser.UserRole;
                //bool isAjax = HttpContext.Request.Headers["X-Requested-With"] == "XMLHttpRequest";
                if (!string.IsNullOrEmpty(keywords))
                {
                    //page = 1;
                    ViewData["keywords"] = keywords;
                    if (userRole == "RMB Super Admin")
                    {
                        int recsCount = _db.Job.Where(x => x.DeleteFlag == 0 && (x.CustCode.Contains(keywords) || x.OldJobNo.Contains(keywords))
                                        ).Count();

                        const int pageSize = 10;
                        if (page < 1)
                            page = 1;

                        var pager = new Pager(recsCount, page, pageSize);
                        int recSkip = (page - 1) * pageSize;

                        var SavedJob = (from x in _db.Job
                                        join y in _db.JobsDetLoc.Where(a => a.Status == "Stored") on x.OldJobNo equals y.JobNo into a
                                        from z in a.DefaultIfEmpty()
                                        where x.DeleteFlag == 0
                                        && (x.CustCode.Contains(keywords)
                                        || x.OldJobNo.Contains(keywords))                                        
                                        orderby x.RequestDate descending
                                        group new { x, z } by new 
                                        { 
                                          x.Id,
                                          x.CustCode,
                                          x.DeptCode,                                           
                                          x.OldJobNo,
                                          x.JobOrderNo,
                                          x.JobType,
                                          x.CtnType,
                                          x.ServLevel,
                                          x.Staff,
                                          x.RequestDate,
                                          x.Remark,
                                          x.JobDate,
                                          z.JobNo } into gp
                                        select new NewJob
                                        {
                                            Id = gp.Key.Id,
                                            CustCode = gp.Key.CustCode,
                                            DeptCode = gp.Key.DeptCode,
                                            OldJobNo = gp.Key.OldJobNo,
                                            JobOrderNo = gp.Key.JobOrderNo,
                                            JobType = gp.Key.JobType,
                                            CtnType = gp.Key.CtnType,                                           
                                            ServLevel = gp.Key.ServLevel,
                                            TotalCtn = gp.Count() != 1 ? gp.Count() : 0,
                                            RequestDate = gp.Key.RequestDate,   
                                            JobDate = gp.Key.JobDate,
                                            Staff = gp.Key.Staff,
                                            Remark = gp.Key.Remark,
                                            OrderFrom = gp.Key.JobOrderNo == "" ? "SOF" : "User"
                                        }).OrderByDescending(x => x.JobDate).Skip(recSkip).Take(pager.PageSize).ToList();
                        this.ViewBag.Pager = pager;
                        //IEnumerable<Job>? SavedJob = _db.Job.OrderBy(x => x.OldJobNo).ToList();
                        model.JobList = (List<NewJob>)SavedJob;
                        //model.carton = _db.JobsDetLoc.Where(a => a.JobNo == SavedJob.OldJobNo).Count();
                        model.CustomerList = new SelectList(_db.Customers, "CustomerCode", "CustomerCode");
                        model.DepartmentList = new SelectList(_db.Departments.Where(y => y.CustomerCode == loginuser.CustomerCode).ToList(), "DepartmentCode", "DepartmentCode");
                        return View(model);
                    }
                    else
                    {
                        int recsCount = _db.Job.Where(x => x.CustCode == loginuser.CustomerCode && x.DeleteFlag == 0 && (x.OldJobNo.Contains(keywords)) && x.DeptCode == ((DepartmentCode == null || DepartmentCode == "") ? x.DeptCode : DepartmentCode)
                                        ).Count();

                        const int pageSize = 10;
                        if (page < 1)
                            page = 1;

                        var pager = new Pager(recsCount, page, pageSize);
                        int recSkip = (page - 1) * pageSize;

                        var SavedJob = (from x in _db.Job
                                        join y in _db.JobsDetLoc.Where(a => a.Status == "Stored") on x.OldJobNo equals y.JobNo into a
                                        from z in a.DefaultIfEmpty()
                                        where x.CustCode == loginuser.CustomerCode
                                        && x.DeleteFlag == 0
                                        && (x.DeptCode.Contains(keywords)
                                        || x.OldJobNo.Contains(keywords))
                                        orderby x.RequestDate descending
                                        group new { x, z } by new
                                        {
                                            x.Id,
                                            x.CustCode,
                                            x.DeptCode,
                                            x.OldJobNo,
                                            x.JobOrderNo,
                                            x.JobType,
                                            x.CtnType,
                                            x.ServLevel,
                                            x.Staff,
                                            x.RequestDate,
                                            x.Remark,
                                            x.JobDate,
                                            z.JobNo
                                        } into gp
                                        select new NewJob
                                        {
                                            Id = gp.Key.Id,
                                            CustCode = gp.Key.CustCode,
                                            DeptCode = gp.Key.DeptCode,
                                            OldJobNo = gp.Key.OldJobNo,
                                            JobOrderNo = gp.Key.JobOrderNo,
                                            JobType = gp.Key.JobType,
                                            CtnType = gp.Key.CtnType,
                                            ServLevel = gp.Key.ServLevel,
                                            TotalCtn = gp.Count() != 1 ? gp.Count() : 0,
                                            RequestDate = gp.Key.RequestDate,
                                            JobDate = gp.Key.JobDate,
                                            Staff = gp.Key.Staff,
                                            Remark = gp.Key.Remark,
                                            OrderFrom = gp.Key.JobOrderNo == "" ? "SOF" : "User"
                                        }).OrderByDescending(x => x.JobDate).Skip(recSkip).Take(pager.PageSize).ToList();
                        this.ViewBag.Pager = pager;
                        model.DepartmentList = new SelectList(_db.Departments.Where(y => y.CustomerCode == loginuser.CustomerCode).ToList(), "DepartmentCode", "DepartmentCode");
                        //IEnumerable<Job>? SavedJob = _db.Job.Where(x => x.CustCode == loginuser.CustomerCode).ToList();
                        model.JobList = (List<NewJob>)SavedJob;
                        return View(model);
                    }
                }
                else
                {
                    ViewData["CustomerCode"] = CustomerCode;
                    ViewData["DepartmentCode"] = DepartmentCode;
                    if (userRole == "RMB Super Admin")
                    {
                        int recsCount = _db.Job.Where(x => x.DeleteFlag == 0 && x.CustCode == ((CustomerCode == null || CustomerCode == "") ? x.CustCode : CustomerCode)
                                        && x.DeptCode == ((DepartmentCode == null || DepartmentCode == "") ? x.DeptCode : DepartmentCode)
                                        ).Count();

                        const int pageSize = 10;
                        if (page < 1)
                            page = 1;

                        var pager = new Pager(recsCount, page, pageSize);
                        int recSkip = (page - 1) * pageSize;
                        ViewData["custcode"] = CustomerCode;
                        var SavedJob = (from x in _db.Job
                                        join y in _db.JobsDetLoc on x.OldJobNo equals y.JobNo into a
                                        from z in a.DefaultIfEmpty()
                                        where x.DeleteFlag == 0
                                        && x.CustCode == ((CustomerCode == null || CustomerCode == "") ? x.CustCode : CustomerCode)
                                        && x.DeptCode == ((DepartmentCode == null || DepartmentCode == "") ? x.DeptCode : DepartmentCode)
                                        orderby x.RequestDate descending
                                        group new { x, z } by new
                                        {
                                            x.Id,
                                            x.CustCode,
                                            x.DeptCode,
                                            x.OldJobNo,
                                            x.JobOrderNo,
                                            x.JobType,
                                            x.CtnType,
                                            x.ServLevel,
                                            x.Staff,
                                            x.RequestDate,
                                            x.Remark,
                                            x.JobDate,                                            
                                            z.JobNo
                                        } into gp 
                                        select new NewJob
                                        {
                                            Id = gp.Key.Id,
                                            CustCode = gp.Key.CustCode,
                                            DeptCode = gp.Key.DeptCode,
                                            OldJobNo = gp.Key.OldJobNo,
                                            JobOrderNo = gp.Key.JobOrderNo,
                                            JobType = gp.Key.JobType,
                                            CtnType = gp.Key.CtnType,
                                            ServLevel = gp.Key.ServLevel,
                                            TotalCtn = gp.Count() != 1 ? gp.Count() : 0,
                                            RequestDate = gp.Key.RequestDate,
                                            JobDate = gp.Key.JobDate,
                                            Staff = gp.Key.Staff,
                                            Remark = gp.Key.Remark,
                                            OrderFrom = gp.Key.JobOrderNo == "" ? "SOF" : "User"
                                        }).OrderByDescending(x => x.JobDate).Skip(recSkip).Take(pager.PageSize).ToList();
                        this.ViewBag.Pager = pager;
                        //IEnumerable<Job>? SavedJob = _db.Job.OrderBy(x => x.OldJobNo).ToList();
                        model.JobList = (List<NewJob>)SavedJob;
                        model.CustomerList = new SelectList(_db.Customers, "CustomerCode", "CustomerCode");
                        model.DepartmentList = new SelectList(_db.Departments.Where(y => y.CustomerCode == loginuser.CustomerCode).ToList(), "DepartmentCode", "DepartmentCode");
                        return View(model);
                    }
                    else
                    {
                        int recsCount = _db.Job.Where(x => x.CustCode == loginuser.CustomerCode && x.DeleteFlag == 0 && x.DeptCode == ((DepartmentCode == null || DepartmentCode == "") ? x.DeptCode : DepartmentCode)
                                        ).Count();

                        const int pageSize = 10;
                        if (page < 1)
                            page = 1;

                        var pager = new Pager(recsCount, page, pageSize);
                        int recSkip = (page - 1) * pageSize;

                        var SavedJob = (from x in _db.Job
                                        join y in _db.JobsDetLoc.Where(a => a.Status == "Stored") on x.OldJobNo equals y.JobNo into a
                                        from z in a.DefaultIfEmpty()
                                        where x.CustCode == loginuser.CustomerCode
                                        && x.DeleteFlag == 0
                                        && x.DeptCode == ((DepartmentCode == null || DepartmentCode == "") ? x.DeptCode : DepartmentCode)
                                        orderby x.RequestDate descending
                                        group new { x, z } by new
                                        {
                                            x.Id,
                                            x.CustCode,
                                            x.DeptCode,
                                            x.OldJobNo,
                                            x.JobOrderNo,
                                            x.JobType,
                                            x.CtnType,
                                            x.ServLevel,
                                            x.Staff,
                                            x.RequestDate,
                                            x.Remark,
                                            x.JobDate,
                                            z.JobNo
                                        } into gp
                                        select new NewJob
                                        {
                                            Id = gp.Key.Id,
                                            CustCode = gp.Key.CustCode,
                                            DeptCode = gp.Key.DeptCode,
                                            OldJobNo = gp.Key.OldJobNo,
                                            JobOrderNo = gp.Key.JobOrderNo,
                                            JobType = gp.Key.JobType,
                                            CtnType = gp.Key.CtnType,
                                            ServLevel = gp.Key.ServLevel,
                                            TotalCtn = gp.Count() != 1 ? gp.Count() : 0,
                                            RequestDate = gp.Key.RequestDate,
                                            JobDate = gp.Key.JobDate,
                                            Staff = gp.Key.Staff,
                                            Remark = gp.Key.Remark,
                                            OrderFrom = gp.Key.JobOrderNo == "" ? "SOF" : "User"
                                        }).OrderByDescending(x => x.JobDate).Skip(recSkip).Take(pager.PageSize).ToList();
                        this.ViewBag.Pager = pager;
                        model.DepartmentList = new SelectList(_db.Departments.Where(y => y.CustomerCode == loginuser.CustomerCode).ToList(), "DepartmentCode", "DepartmentCode");
                        //IEnumerable<Job>? SavedJob = _db.Job.Where(x => x.CustCode == loginuser.CustomerCode).ToList();
                        model.JobList = (List<NewJob>)SavedJob;
                        return View(model);
                    }
                }
            }
            else
            {
                return Redirect("~/Identity/Account/Login/");
            }
        }
        [HttpPost]
        public JsonResult CheckRecord(string department, string recordno)
        {
            var record = _db.OrderDetail.Where(x => x.DepartmentCode == department && x.RecordNo == recordno).FirstOrDefault();
            int result = 0;
            if (record == null)
            {
                result = 0;
            }
            else
            {
                result = 1;
            }
            return Json(result);
        }

        [HttpPost]
        public JsonResult CustInfo(string custcode)
        {
            JobCollect model = new JobCollect();
            model.CustomerBill = (from c in _db.Customers
                              join b in _db.BillRateMaster on c.CustomerCode equals b.CustomerCode
                              where b.CustomerCode == custcode
                              select new CustomerBill
                              {
                                  customer = c,
                                  billratemaster = b
                              }).FirstOrDefault();            
            model.DepartmentList = new SelectList(_db.Departments.Where(y => y.CustomerCode == custcode).ToList(), "DepartmentCode", "DepartmentCode");            
            return Json(model);
        }

        [HttpPost]
        public JsonResult Orders(int ordercode, string custcode)
        {
            var orderDb = _db.OrderRequests.Where(x => x.CustomerCode == custcode && x.OrderId == ordercode).FirstOrDefault();
            return Json(orderDb);
        }

        public async Task<IActionResult> NoRegisJobsAsync(int page, string keywords = "")
        {
            checkUser();
            if (loginflag == true)
            {
                var loginuser = await _userManager.GetUserAsync(User);
                GroupedCustDept model = new GroupedCustDept();
                string userRole = loginuser.UserRole;
                if (!string.IsNullOrEmpty(keywords))
                {
                    page = 1;
                    ViewData["keywords"] = keywords;
                    var JobRequest = (from x in _db.Job
                                      where x.CustCode.Contains(keywords)
                                      || x.JobOrderNo.Contains(keywords)
                                      select x.JobOrderNo).ToList();

                    var OrderRequest = (from x in _db.OrderRequests
                                        where x.OrderStatus == "Accepted"
                                        && (x.CustomerCode.Contains(keywords)
                                        || x.OrderId.ToString().Contains(keywords))
                                        orderby x.OrderId descending
                                        select x).ToList();

                    int recsCount = (from e in OrderRequest
                                     where !(from m in JobRequest
                                             select m).Contains(e.OrderId.ToString())
                                     select e).Count();
                    const int pageSize = 10;
                    if (page < 1)
                        page = 1;

                    var pager = new Pager(recsCount, page, pageSize);
                    int recSkip = (page - 1) * pageSize;
                    var OrderList2 = (from e in OrderRequest
                                      where !(from m in JobRequest
                                              select m).Contains(e.OrderId.ToString())
                                      orderby e.OrderId descending
                                      select e).Skip(recSkip).Take(pager.PageSize).ToList();

                    this.ViewBag.Pager = pager;
                    model.OrderList = (List<OrderRequests>)OrderList2;
                    model.CustomerList = new SelectList(_db.Customers, "CustomerCode", "CustomerCode");
                    return View(model);
                }
                else
                {
                    int recsCount = (from e in _db.OrderRequests
                                     where e.OrderStatus == "Accepted"
                                      && !(from m in _db.Job
                                           select m.JobOrderNo).Contains(e.OrderId.ToString())
                                     select e).Count();
                    const int pageSize = 10;
                    if (page < 1)
                        page = 1;

                    var pager = new Pager(recsCount, page, pageSize);
                    int recSkip = (page - 1) * pageSize;
                    var OrderList2 = (from e in _db.OrderRequests
                                      where e.OrderStatus == "Accepted"
                                      && !(from m in _db.Job
                                           select m.JobOrderNo).Contains(e.OrderId.ToString())
                                      orderby e.OrderId descending
                                      select e).Skip(recSkip).Take(pager.PageSize).ToList();

                    this.ViewBag.Pager = pager;
                    model.OrderList = (List<OrderRequests>)OrderList2;
                    model.CustomerList = new SelectList(_db.Customers, "CustomerCode", "CustomerCode");
                    return View(model);
                }
            }
            else
            {
                return Redirect("~/Identity/Account/Login/");
            }
        }

        public async Task<IActionResult> TransactionAsync(int page, string keywords = "", string CustomerCode = "", string DepartmentCode = "")
        {
            checkUser();
            if (loginflag == true)
            {
                var loginuser = await _userManager.GetUserAsync(User);
                NewGroupedCustDept model = new NewGroupedCustDept();
                string userRole = loginuser.UserRole;
                if (!string.IsNullOrEmpty(keywords))
                {
                    //page = 1;
                    ViewData["keywords"] = keywords;
                    if (userRole == "RMB Super Admin")
                    {
                        int recsCount = (from e in _db.OrderRequests
                                         where (e.CustomerCode.Contains(keywords)
                                         || e.CustomerName.Contains(keywords)
                                         || e.OrderId.ToString().Contains(keywords)
                                         )
                                         select e).Count();
                        const int pageSize = 10;
                        if (page < 1)
                            page = 1;

                        var pager = new Pager(recsCount, page, pageSize);
                        int recSkip = (page - 1) * pageSize;

                        var OrderRequest = (from x in _db.OrderRequests
                                            where (x.CustomerCode.Contains(keywords)
                                            || x.CustomerName.Contains(keywords)
                                            || x.OrderId.ToString().Contains(keywords)
                                            )
                                            orderby x.OrderId descending
                                            select new NewOrderRequests
                                            {
                                                Id = x.Id,
                                                OrderId = x.OrderId,
                                                OrderStatus = x.OrderStatus,
                                                CustomerCode = x.CustomerCode,
                                                CartonQty = x.CartonQty,
                                                OrderDate = x.OrderDate,
                                                OrderType = x.OrderType,
                                                ContactPerson = x.ContactPerson,
                                                ApproverId = "No"
                                            }).Skip(recSkip).Take(pager.PageSize).ToList();

                        this.ViewBag.Pager = pager;
                        model.OrderList = (List<NewOrderRequests>)OrderRequest;
                        model.CustomerList = new SelectList(_db.Customers, "CustomerCode", "CustomerCode");
                        return View(model);
                    }
                    else
                    {
                        var AppId = "No";
                        var cust = (from a in _db.Customers
                                    where a.CustomerCode == model.OrderRequest.CustomerCode
                                    select a).FirstOrDefault();
                        var approvesetup = (from x in _db.OrderRequests
                                            join y in _db.ApprovalSetup on x.UserId equals y.UserId
                                            where y.ApproverId == loginuser.UserCode
                                            select y).ToList();
                        if (cust.NeedApproval == "No" || approvesetup == null)
                        {
                            AppId = "No";
                        }
                        else
                        {
                            AppId = "Yes";
                        }
                        int recsCount = (from x in _db.OrderRequests
                                         //join y in _db.ApprovalSetup on x.UserId equals y.UserId
                                         where x.CustomerCode == loginuser.CustomerCode
                                         && (x.DepartmentCode.Contains(keywords)
                                         || x.OrderId.ToString().Contains(keywords))
                                         //&& y.ApproverId == loginuser.UserCode
                                         orderby x.OrderId descending
                                         select x).Count();
                        const int pageSize = 10;
                        if (page < 1)
                            page = 1;

                        var pager = new Pager(recsCount, page, pageSize);
                        int recSkip = (page - 1) * pageSize;

                        //var OrderRequest = (from x in _db.OrderRequests
                        //                    where x.CustomerCode == loginuser.CustomerCode
                        //                    && (x.DepartmentCode.Contains(keywords)
                        //                    || x.OrderId.ToString().Contains(keywords))
                        //                    orderby x.OrderId descending
                        //                    select new NewOrderRequests
                        //                    {
                        //                        Id = x.Id,
                        //                        OrderId = x.OrderId,
                        //                        OrderStatus = x.OrderStatus,
                        //                        CustomerCode = x.CustomerCode,
                        //                        CartonQty = x.CartonQty,
                        //                        OrderDate = x.OrderDate,
                        //                        OrderType = x.OrderType,
                        //                        ContactPerson = x.ContactPerson,
                        //                        ApproverId = AppId
                        //                    }).Skip(recSkip).Take(pager.PageSize).ToList();
                        var OrderRequestApp = (from x in _db.OrderRequests
                                               //join y in _db.ApprovalSetup on x.UserId equals y.UserId
                                               where x.CustomerCode == loginuser.CustomerCode
                                               && (x.DepartmentCode.Contains(keywords)
                                               || x.OrderId.ToString().Contains(keywords))
                                               //&& y.ApproverId == loginuser.UserCode
                                               orderby x.OrderId descending
                                               select new NewOrderRequests
                                               {
                                                   Id = x.Id,
                                                   OrderId = x.OrderId,
                                                   OrderStatus = x.OrderStatus,
                                                   CustomerCode = x.CustomerCode,
                                                   CartonQty = x.CartonQty,
                                                   OrderDate = x.OrderDate,
                                                   OrderType = x.OrderType,
                                                   ContactPerson = x.ContactPerson,
                                                   ApproverId = AppId
                                               }).Skip(recSkip).Take(pager.PageSize).ToList();
                        this.ViewBag.Pager = pager;
                        model.Customer = (from x in _db.Customers
                                          where x.CustomerCode == loginuser.CustomerCode
                                          select x).FirstOrDefault();
                        model.DepartmentList = new SelectList(_db.Departments.Where(y => y.CustomerCode == loginuser.CustomerCode).ToList(), "DepartmentCode", "DepartmentCode");
                        model.OrderList = (List<NewOrderRequests>)OrderRequestApp;
                        return View(model);
                    }
                }
                else
                {
                    ViewData["CustomerCode"] = CustomerCode;
                    ViewData["DepartmentCode"] = DepartmentCode;
                    if (userRole == "RMB Super Admin")
                    {
                        int recsCount = (from e in _db.OrderRequests
                                         where e.CustomerCode == ((CustomerCode == null || CustomerCode == "") ? e.CustomerCode : CustomerCode)
                                         && e.DepartmentCode == ((DepartmentCode == null || DepartmentCode == "") ? e.DepartmentCode : DepartmentCode)
                                         select e).Count();
                        const int pageSize = 10;
                        if (page < 1)
                            page = 1;

                        var pager = new Pager(recsCount, page, pageSize);
                        int recSkip = (page - 1) * pageSize;

                        var OrderRequest = (from x in _db.OrderRequests
                                            where x.CustomerCode == ((CustomerCode == null || CustomerCode == "") ? x.CustomerCode : CustomerCode)
                                            && x.DepartmentCode == ((DepartmentCode == null || DepartmentCode == "") ? x.DepartmentCode : DepartmentCode)
                                            orderby x.OrderId descending
                                            select new NewOrderRequests
                                            {
                                                Id = x.Id,
                                                OrderId = x.OrderId,
                                                OrderStatus = x.OrderStatus,
                                                CustomerCode = x.CustomerCode,
                                                CartonQty = x.CartonQty,
                                                OrderDate = x.OrderDate,
                                                OrderType = x.OrderType,
                                                ContactPerson = x.ContactPerson,
                                                ApproverId = "No"
                                            }).Skip(recSkip).Take(pager.PageSize).ToList();

                        this.ViewBag.Pager = pager;
                        model.OrderList = (List<NewOrderRequests>)OrderRequest;
                        model.CustomerList = new SelectList(_db.Customers, "CustomerCode", "CustomerCode");
                        return View(model);
                    }
                    else
                    {
                        var AppId = "No";
                        var cust = (from a in _db.Customers
                                    where a.CustomerCode == loginuser.CustomerCode
                                    select a).FirstOrDefault();
                        var approvesetup = (from x in _db.OrderRequests
                                            join y in _db.ApprovalSetup on x.UserId equals y.UserId
                                            where y.ApproverId == loginuser.UserCode
                                            select y).ToList();
                        if (cust.NeedApproval == "No" || approvesetup.Count == 0)
                        {
                            AppId = "No";
                        }
                        else
                        {
                            AppId = "Yes";
                        }
                        int recsCount = (from x in _db.OrderRequests
                                         //join y in _db.ApprovalSetup on x.UserId equals y.UserId
                                         where x.CustomerCode == loginuser.CustomerCode
                                         && x.DepartmentCode == ((DepartmentCode == null || DepartmentCode == "") ? x.DepartmentCode : DepartmentCode)
                                         //&& y.ApproverId == loginuser.UserCode
                                         orderby x.OrderId descending
                                         select x).Count();
                        const int pageSize = 10;
                        if (page < 1)
                            page = 1;

                        var pager = new Pager(recsCount, page, pageSize);
                        int recSkip = (page - 1) * pageSize;

                        //var OrderRequest = (from x in _db.OrderRequests
                        //                    where x.CustomerCode == loginuser.CustomerCode
                        //                    && x.DepartmentCode == ((DepartmentCode == null || DepartmentCode == "") ? x.DepartmentCode : DepartmentCode)
                        //                    orderby x.OrderId descending
                        //                    select x).Skip(recSkip).Take(pager.PageSize).ToList();
                        var OrderRequestApp = (from x in _db.OrderRequests
                                               //join y in _db.ApprovalSetup on x.UserId equals y.UserId
                                               where x.CustomerCode == loginuser.CustomerCode
                                               && x.DepartmentCode == ((DepartmentCode == null || DepartmentCode == "") ? x.DepartmentCode : DepartmentCode)
                                               //&& y.ApproverId == loginuser.UserCode
                                               orderby x.OrderId descending
                                               select new NewOrderRequests
                                               {
                                                   Id = x.Id,
                                                   OrderId = x.OrderId,
                                                   OrderStatus = x.OrderStatus,
                                                   CustomerCode = x.CustomerCode,
                                                   CartonQty = x.CartonQty,
                                                   OrderDate = x.OrderDate,
                                                   OrderType = x.OrderType,
                                                   ContactPerson = x.ContactPerson,
                                                   ApproverId = AppId
                                               }).Skip(recSkip).Take(pager.PageSize).ToList();
                        this.ViewBag.Pager = pager;
                        model.DepartmentList = new SelectList(_db.Departments.Where(y => y.CustomerCode == loginuser.CustomerCode).ToList(), "DepartmentCode", "DepartmentCode");
                        model.OrderList = (List<NewOrderRequests>)OrderRequestApp;
                        return View(model);
                    }
                }
            }
            else
            {
                return Redirect("~/Identity/Account/Login/");
            }
        }

        [HttpPost]
        public JsonResult GetDept(string custcode)
        {
            SelectList DepartmentList = new SelectList(_db.Departments.Where(y => y.CustomerCode == custcode).ToList(), "DepartmentCode", "DepartmentCode");
            return Json(DepartmentList);
        }
        public async Task<IActionResult> JobOrderEAsync()
        {
            checkUser();
            if (loginflag == true)
            {
                var loginuser = await _userManager.GetUserAsync(User);
                GroupedCustDept model = new GroupedCustDept();

                if (loginuser.UserRole == "RMB Super Admin")
                {
                    model.CustomerList = new SelectList(_db.Customers, "CustomerCode", "CustomerCode");
                }
                else
                {
                    var customerDb = _db.Customers.Where(x => x.CustomerCode == loginuser.CustomerCode).FirstOrDefault();
                    if (String.IsNullOrEmpty(loginuser.DepartmentPermission))
                    {
                        model.DepartmentList = new SelectList(_db.Departments.Where(y => y.CustomerCode == loginuser.CustomerCode).ToList(), "DepartmentCode", "DepartmentCode");
                    }
                    else
                    {
                        string PerDeptList = loginuser.DepartmentPermission;
                        char[] separators = new char[] { ' ', ',' };
                        string[] subs = PerDeptList.Split(separators, StringSplitOptions.RemoveEmptyEntries);
                        model.DepartmentList = new SelectList(subs);
                    }
                    if (customerDb == null)
                    {
                        return NotFound();
                    }
                    model.Customer = customerDb;
                }
                return View(model);
            }
            else
            {
                return Redirect("~/Identity/Account/Login/");
            }
        }
        public async Task<IActionResult> JobOrderRAsync()
        {
            checkUser();
            if (loginflag == true)
            {
                var loginuser = await _userManager.GetUserAsync(User);
                GroupedCustDept model = new GroupedCustDept();

                if (loginuser.UserRole == "RMB Super Admin")
                {
                    model.CustomerList = new SelectList(_db.Customers, "CustomerCode", "CustomerCode");
                }
                else
                {
                    var customerDb = _db.Customers.Where(x => x.CustomerCode == loginuser.CustomerCode).FirstOrDefault();
                    if (String.IsNullOrEmpty(loginuser.DepartmentPermission))
                    {
                        model.PerDeptList = new SelectList(_db.Departments.Where(y => y.CustomerCode == loginuser.CustomerCode).ToList(), "DepartmentCode", "DepartmentCode");
                        model.DepartmentList = new SelectList(_db.Departments.Where(y => y.CustomerCode == loginuser.CustomerCode).ToList(), "DepartmentCode", "DepartmentCode");
                    }
                    else
                    {
                        string PerDeptList = loginuser.DepartmentPermission;
                        char[] separators = new char[] { ' ', ',' };
                        string[] subs = PerDeptList.Split(separators, StringSplitOptions.RemoveEmptyEntries);
                        model.DepartmentList = new SelectList(subs);
                        model.PerDeptList = new SelectList(subs);
                    }

                    if (customerDb == null)
                    {
                        return NotFound();
                    }
                    model.Customer = customerDb;
                }
                return View(model);
            }
            else
            {
                return Redirect("~/Identity/Account/Login/");
            }
        }
        [HttpPost]
        public JsonResult Cust(string custcode)
        {
            //Custcode = custcode;
            GroupedCustDept model = new GroupedCustDept();
            var customerDb = _db.Customers.Where(x => x.CustomerCode == custcode).FirstOrDefault();
            model.DepartmentList = new SelectList(_db.Departments.Where(y => y.CustomerCode == custcode).ToList(), "DepartmentCode", "DepartmentCode");
            if (customerDb != null)
            {
                model.Customer = customerDb;
            }
            return Json(model);
            //_ = JobOrderAsync();
        }
        [HttpPost]
        public void InsertJobList(string data, string odlist)
        {
            joblist = data;
            orderlist = odlist;
        }
        [HttpPost]
        public void InsertOrderList(string data)
        {
            orderlist = data;
        }

        //POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> FOrderEmptyCartonsAsync(GroupedCustDept model)
        {
            checkUser();
            if (loginflag == true)
            {
                var loginuser = await _userManager.GetUserAsync(User);
                ModelState.Remove("Customer");
                ModelState.Remove("OrderList");
                ModelState.Remove("DepartmentList");
                ModelState.Remove("CustomerList");
                ModelState.Remove("OrderRequest.UserId");
                ModelState.Remove("OrderRequest.Remark");
                ModelState.Remove("OrderRequest.TieQty");
                ModelState.Remove("OrderRequest.RICQty");
                ModelState.Remove("OrderRequest.PlasticBagQty");
                ModelState.Remove("OrderRequest.TamperSealQty");
                ModelState.Remove("OrderRequest.Telephone");
                ModelState.Remove("OrderRequest.OrderId");
                ModelState.Remove("OrderRequest.OrderStatus");
                ModelState.Remove("OrderRequest.OrderType");
                ModelState.Remove("OrderRequest.COrdBarCodeId");
                ModelState.Remove("OrderRequest.DepartmentCode");
                ModelState.Remove("OrderDetail");
                ModelState.Remove("OrderDetailList");
                ModelState.Remove("PerDeptList");
                if (ModelState.IsValid)
                {
                    model.OrderRequest.UserId = loginuser.UserCode;
                    var lastOrderId = _db.OrderRequests.Max(x => x.OrderId);
                    model.OrderRequest.OrderId = lastOrderId + 1;
                    model.OrderRequest.DepartmentCode = Request.Form["DepartmentCode"];
                    model.OrderRequest.OrderType = "1";

                    //TempData["success"] = "Your order is being processed.Your order number is " + (lastOrderId + 1) + ".You will receive a confirmation email with all your order information soon.";

                    var cust = (from a in _db.Customers
                                where a.CustomerCode == model.OrderRequest.CustomerCode
                                select a).FirstOrDefault();
                    var approvesetup = (from b in _db.ApprovalSetup
                                        where b.UserId == model.OrderRequest.UserId
                                        select b).FirstOrDefault();
                    if (cust.NeedApproval == "No" || approvesetup == null)
                    {
                        model.OrderRequest.OrderStatus = "Accepted";
                        string Subject = "Mitsui-Soko RMB service order (" + model.OrderRequest.OrderId + ") is approved";
                        string Body = "<div style='font-size: 14px'><table class='nth-table table table-hover pt-3'>"
                            + "<tr><td>Order No:</td><td> " + model.OrderRequest.OrderId
                            + "</td></tr>"
                            + "<tr><td>Customer Code:</td><td> " + model.OrderRequest.CustomerCode
                            + "</td></tr>"
                            + "<tr><td>Company Name:</td><td> " + model.OrderRequest.CustomerName
                            + "</td></tr>"
                            + "<tr><td>Company Address:</td><td> " + model.OrderRequest.Address1
                            + "</td></tr>"
                            + "<tr><td></td><td> " + model.OrderRequest.Address2 + "</td></tr>"
                            + "<tr><td></td><td> " + model.OrderRequest.Address3 + "</td></tr>"
                            + "<tr><td>Postal Code:</td><td> " + model.OrderRequest.Address4
                            + "</td></tr>"
                            + "<tr><td>Department:</td><td> " + model.OrderRequest.DepartmentCode
                            + "</td></tr>"
                            + "<tr><td>Contact Person:</td><td> " + model.OrderRequest.ContactPerson
                            + "</td></tr>"
                            + "<tr><td>Contact Phone No:</td><td> " + model.OrderRequest.Telephone
                            + "</td></tr>"
                            + "<tr><td>Request Date:</td><td> " + model.OrderRequest.OrderDate
                            + "</td></tr>"
                            + "<tr><td>Remarks:</td><td> " + model.OrderRequest.Remark
                            + "</td></tr><br>"
                            + "Yours, <br>"
                            + "Mitsui-Soko Records Management <br></div>"
                            + "<p style='padding: 0; font-size:11px; color:#375a7f;'>Remark: This email is auto generated by Mitsui-Soko Records Management System. " +
                            "This message including any attachments may contain confidential " +
                            "or legally privileged information. Any unauthorized use, retention, reproduction " +
                            "or disclosure is prohibited and may attract civil and criminal penalties. If this e-mail has been sent to you in error, please delete it and notify us immediately."
                            + "</p>";
                        SendEmail("chaw@mitsui-soko.com.sg", Subject, Body);
                    }
                    else
                    {
                        model.OrderRequest.OrderStatus = "Pending";
                        string host = HttpContext.Request.Host.Value;
                        var code = await _userManager.GeneratePasswordResetTokenAsync(loginuser); //, Options.Tokens.AuthenticatorTokenProvider, "ApproveOrder");
                        code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
                        var callbackUrl = $"{HttpContext.Request.Scheme}://{HttpContext.Request.Host}/Order/ApproveOrder/{model.OrderRequest.OrderId}";
                        string Subject = "Mitsui-Soko RMB service order (" + model.OrderRequest.OrderId + ") need approval";
                        string Body = "<div style='font-size: 14px'><table class='nth-table table table-hover pt-3'>"
                                + "<tr><td>Order No:</td><td> " + model.OrderRequest.OrderId
                                + "</td></tr>"
                                + "<tr><td>Customer Code:</td><td> " + model.OrderRequest.CustomerCode
                                + "</td></tr>"
                                + "<tr><td>Company Name:</td><td> " + model.OrderRequest.CustomerName
                                + "</td></tr>"
                                + "<tr><td>Company Address:</td><td> " + model.OrderRequest.Address1
                                + "</td></tr>"
                                + "<tr><td></td><td> " + model.OrderRequest.Address2 + "</td></tr>"
                                + "<tr><td></td><td> " + model.OrderRequest.Address3 + "</td></tr>"
                                + "<tr><td>Postal Code:</td><td> " + model.OrderRequest.Address4
                                + "</td></tr>"
                                + "<tr><td>Department:</td><td> " + model.OrderRequest.DepartmentCode
                                + "</td></tr>"
                                + "<tr><td>Contact Person:</td><td> " + model.OrderRequest.ContactPerson
                                + "</td></tr>"
                                + "<tr><td>Contact Phone No:</td><td> " + model.OrderRequest.Telephone
                                + "</td></tr>"
                                + "<tr><td>Request Date:</td><td> " + model.OrderRequest.OrderDate
                                + "</td></tr>"
                                + "<tr><td>Remarks:</td><td> " + model.OrderRequest.Remark
                                + "</td></tr><br>"
                                + "<br>"
                                + "<tr><td><a href='" + callbackUrl + "'>Approve</a> | <a href='" + callbackUrl + "'>Reject</a></td></tr>"
                                + "<br>"
                                + "Yours, <br>"
                                + "Mitsui-Soko Records Management <br></div>"
                                + "<p style='padding: 0; font-size:11px; color:#375a7f;'>Remark: This email is auto generated by Mitsui-Soko Records Management System. " +
                                "This message including any attachments may contain confidential " +
                                "or legally privileged information. Any unauthorized use, retention, reproduction " +
                                "or disclosure is prohibited and may attract civil and criminal penalties. If this e-mail has been sent to you in error, please delete it and notify us immediately."
                                + "</p>";
                        var approver = (from a in _db.AppUsers
                                        where a.UserName == approvesetup.ApproverId
                                        select a.Name).FirstOrDefault();
                        var backupapprover = (from a in _db.AppUsers
                                              where a.UserName == approvesetup.SubApproverId
                                              select a.Name).FirstOrDefault();
                        var approveremail = (from a in _db.AppUsers
                                             where a.UserName == approvesetup.ApproverId
                                             select a.Email).FirstOrDefault();
                        var backupapproveremail = (from a in _db.AppUsers
                                                   where a.UserName == approvesetup.SubApproverId
                                                   select a.Email).FirstOrDefault();
                        if (approveremail != null)
                        {
                            SendEmail(approveremail, Subject, Body);
                        }
                        else
                        {
                            SendEmail(approvesetup.Email, Subject, Body);
                        }
                    }
                    _db.OrderRequests.Add(model.OrderRequest);
                    _db.SaveChanges();
                    return RedirectToAction("Transaction");
                }
                return View(model);
            }
            else
            {
                return Redirect("~/Identity/Account/Login/");
            }
        }

        //POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> FRetrieveRecordRequestAsync(GroupedCustDept model)
        {
            checkUser();
            if (loginflag == true)
            {
                var loginuser = await _userManager.GetUserAsync(User);
                ModelState.Remove("Customer");
                ModelState.Remove("OrderList");
                ModelState.Remove("DepartmentList");
                ModelState.Remove("CustomerList");
                ModelState.Remove("OrderRequest.UserId");
                ModelState.Remove("OrderRequest.Remark");
                ModelState.Remove("OrderRequest.TieQty");
                ModelState.Remove("OrderRequest.RICQty");
                ModelState.Remove("OrderRequest.PlasticBagQty");
                ModelState.Remove("OrderRequest.TamperSealQty");
                ModelState.Remove("OrderRequest.OrderId");
                ModelState.Remove("OrderRequest.OrderStatus");
                ModelState.Remove("OrderRequest.OrderType");
                ModelState.Remove("OrderRequest.COrdBarCodeId");
                ModelState.Remove("OrderRequest.DepartmentCode");
                ModelState.Remove("OrderDetail");
                ModelState.Remove("OrderDetailList");
                ModelState.Remove("PerDeptList");
                if (ModelState.IsValid)
                {
                    model.OrderRequest.UserId = loginuser.UserCode;
                    var lastOrderId = _db.OrderRequests.Max(x => x.OrderId);
                    model.OrderRequest.OrderId = lastOrderId + 1;
                    model.OrderRequest.DepartmentCode = Request.Form["OrderRequest.DepartmentCode"];
                    model.OrderRequest.OrderType = "3";
                    model.OrderRequest.CartonQty = 1;
                    if (orderlist != "")
                    {
                        List<RecordModel>? OrderDetailList = (List<RecordModel>?)JsonSerializer.Deserialize<IList<RecordModel>>(orderlist);
                        string OrderTable = "";
                        //Loop and insert records.
                        if (OrderDetailList != null)
                        {
                            OrderTable += "<div style='font-size: 14px'><h4>Transaction Detail</h4><h4>Retrieve Records</h4><table class='nth-table table table-hover pt-3'>"
                                + "<tr><th>#</th><th>Department Code</th><th>Carton No</th></tr>";
                            foreach (RecordModel od in OrderDetailList)
                            {
                                OrderDetail odetail = new OrderDetail();
                                odetail.OrderId = lastOrderId + 1;
                                odetail.DepartmentCode = od.DepartmentCode;
                                odetail.RecordNo = od.RecordNo;
                                odetail.DestructionDate = new DateTime();
                                _db.OrderDetail.Add(odetail);
                                _db.SaveChanges();
                                OrderTable += "<tr><td>" + od.Id + "</td><td>" + odetail.DepartmentCode + "</td><td>"
                                + odetail.RecordNo + "</td></tr>";
                            }
                            OrderTable += "</table></div>";
                        }

                        //TempData["success"] = "Your order is being processed.Your order number is " + (lastOrderId + 1) + ".You will receive a confirmation email with all your order information soon.";

                        var cust = (from a in _db.Customers
                                    where a.CustomerCode == model.OrderRequest.CustomerCode
                                    select a).FirstOrDefault();
                        var approvesetup = (from b in _db.ApprovalSetup
                                            where b.UserId == model.OrderRequest.UserId
                                            select b).FirstOrDefault();
                        if (cust.NeedApproval == "No" || approvesetup == null)
                        {
                            model.OrderRequest.OrderStatus = "Accepted";
                            string Subject = "Mitsui-Soko RMB service order (" + model.OrderRequest.OrderId + ") is approved";
                            string Body = "<div style='font-size: 14px'><table class='nth-table table table-hover pt-3'>"
                            + "<tr><td>Order No:</td><td> " + model.OrderRequest.OrderId
                            + "</td></tr>"
                            + "<tr><td>Customer Code:</td><td> " + model.OrderRequest.CustomerCode
                            + "</td></tr>"
                            + "<tr><td>Company Name:</td><td> " + model.OrderRequest.CustomerName
                            + "</td></tr>"
                            + "<tr><td>Company Address:</td><td> " + model.OrderRequest.Address1
                            + "</td></tr>"
                            + "<tr><td></td><td> " + model.OrderRequest.Address2 + "</td></tr>"
                            + "<tr><td></td><td> " + model.OrderRequest.Address3 + "</td></tr>"
                            + "<tr><td>Postal Code:</td><td> " + model.OrderRequest.Address4
                            + "</td></tr>"
                            + "<tr><td>Department:</td><td> " + model.OrderRequest.DepartmentCode
                            + "</td></tr>"
                            + "<tr><td>Contact Person:</td><td> " + model.OrderRequest.ContactPerson
                            + "</td></tr>"
                            + "<tr><td>Contact Phone No:</td><td> " + model.OrderRequest.Telephone
                            + "</td></tr>"
                            + "<tr><td>Request Date:</td><td> " + model.OrderRequest.OrderDate
                            + "</td></tr>"
                            + "<tr><td>Remarks:</td><td> " + model.OrderRequest.Remark
                            + "</td></tr><br>"
                            + OrderTable + "<br>"
                            + "Yours, <br>"
                            + "Mitsui-Soko Records Management <br></div>"
                            + "<p style='padding: 0; font-size:11px; color:#375a7f;'>Remark: This email is auto generated by Mitsui-Soko Records Management System. " +
                            "This message including any attachments may contain confidential " +
                            "or legally privileged information. Any unauthorized use, retention, reproduction " +
                            "or disclosure is prohibited and may attract civil and criminal penalties. If this e-mail has been sent to you in error, please delete it and notify us immediately."
                            + "</p>";
                            SendEmail("chaw@mitsui-soko.com.sg", Subject, Body);
                        }
                        else
                        {
                            model.OrderRequest.OrderStatus = "Pending";
                            string host = HttpContext.Request.Host.Value;
                            var callbackUrl = $"{HttpContext.Request.Scheme}://{HttpContext.Request.Host}/Order/ApproveOrder/{model.OrderRequest.OrderId}";

                            string Subject = "Mitsui-Soko RMB service order (" + model.OrderRequest.OrderId + ") need approval";
                            string Body = "<div style='font-size: 14px'><table class='nth-table table table-hover pt-3'>"
                                    + "<tr><td>Order No:</td><td> " + model.OrderRequest.OrderId
                                    + "</td></tr>"
                                    + "<tr><td>Customer Code:</td><td> " + model.OrderRequest.CustomerCode
                                    + "</td></tr>"
                                    + "<tr><td>Company Name:</td><td> " + model.OrderRequest.CustomerName
                                    + "</td></tr>"
                                    + "<tr><td>Company Address:</td><td> " + model.OrderRequest.Address1
                                    + "</td></tr>"
                                    + "<tr><td></td><td> " + model.OrderRequest.Address2 + "</td></tr>"
                                    + "<tr><td></td><td> " + model.OrderRequest.Address3 + "</td></tr>"
                                    + "<tr><td>Postal Code:</td><td> " + model.OrderRequest.Address4
                                    + "</td></tr>"
                                    + "<tr><td>Department:</td><td> " + model.OrderRequest.DepartmentCode
                                    + "</td></tr>"
                                    + "<tr><td>Contact Person:</td><td> " + model.OrderRequest.ContactPerson
                                    + "</td></tr>"
                                    + "<tr><td>Contact Phone No:</td><td> " + model.OrderRequest.Telephone
                                    + "</td></tr>"
                                    + "<tr><td>Request Date:</td><td> " + model.OrderRequest.OrderDate
                                    + "</td></tr>"
                                    + "<tr><td>Remarks:</td><td> " + model.OrderRequest.Remark
                                    + "</td></tr><br>"
                                    + OrderTable + "<br>"
                                    + "<tr><td><a href='" + callbackUrl + "'>Approve</a> | <a href='" + callbackUrl + "'>Reject</a></td></tr>"
                                    + "<br>"
                                    + "Yours, <br>"
                                    + "Mitsui-Soko Records Management <br></div>"
                                    + "<p style='padding: 0; font-size:11px; color:#375a7f;'>Remark: This email is auto generated by Mitsui-Soko Records Management System. " +
                                    "This message including any attachments may contain confidential " +
                                    "or legally privileged information. Any unauthorized use, retention, reproduction " +
                                    "or disclosure is prohibited and may attract civil and criminal penalties. If this e-mail has been sent to you in error, please delete it and notify us immediately."
                                    + "</p>";
                            var approver = (from a in _db.AppUsers
                                            where a.UserName == approvesetup.ApproverId
                                            select a.Name).FirstOrDefault();
                            var backupapprover = (from a in _db.AppUsers
                                                  where a.UserName == approvesetup.SubApproverId
                                                  select a.Name).FirstOrDefault();
                            var approveremail = (from a in _db.AppUsers
                                                 where a.UserName == approvesetup.ApproverId
                                                 select a.Email).FirstOrDefault();
                            var backupapproveremail = (from a in _db.AppUsers
                                                       where a.UserName == approvesetup.SubApproverId
                                                       select a.Email).FirstOrDefault();
                            if (approveremail != null)
                            {
                                SendEmail(approveremail, Subject, Body);
                            }
                            else
                            {
                                SendEmail(approvesetup.Email, Subject, Body);
                            }
                        }
                        _db.OrderRequests.Add(model.OrderRequest);
                        _db.SaveChanges();
                        return RedirectToAction("Transaction");
                    }
                    else
                    {
                        TempData["error"] = "Record No. must be added.";
                        model.CustomerList = new SelectList(_db.Customers, "CustomerCode", "CustomerCode");
                        return Redirect(Request.Headers["Referer"].ToString());
                    }
                }
                return View(model);
            }
            else
            {
                return Redirect("~/Identity/Account/Login/");
            }
        }

        public async Task<IActionResult> JobCollectEAsync()
        {
            checkUser();
            if (loginflag == true)
            {
                var loginuser = await _userManager.GetUserAsync(User);
                GroupedCustDept model = new GroupedCustDept();

                if (loginuser.UserRole == "RMB Super Admin")
                {
                    model.CustomerList = new SelectList(_db.Customers, "CustomerCode", "CustomerCode");
                }
                else
                {
                    var customerDb = _db.Customers.Where(x => x.CustomerCode == loginuser.CustomerCode).FirstOrDefault();
                    if (String.IsNullOrEmpty(loginuser.DepartmentPermission))
                    {
                        model.DepartmentList = new SelectList(_db.Departments.Where(y => y.CustomerCode == loginuser.CustomerCode).ToList(), "DepartmentCode", "DepartmentCode");
                        model.PerDeptList = new SelectList(_db.Departments.Where(y => y.CustomerCode == loginuser.CustomerCode).ToList(), "DepartmentCode", "DepartmentCode");
                    }
                    else
                    {
                        string PerDeptList = loginuser.DepartmentPermission;
                        char[] separators = new char[] { ' ', ',' };
                        string[] subs = PerDeptList.Split(separators, StringSplitOptions.RemoveEmptyEntries);
                        model.DepartmentList = new SelectList(subs);
                        model.PerDeptList = new SelectList(subs);
                    }

                    if (customerDb == null)
                    {
                        return NotFound();
                    }
                    model.Customer = customerDb;
                }
                return View(model);
            }
            else
            {
                return Redirect("~/Identity/Account/Login/");
            }
        }

        public async Task<IActionResult> JobCollectRAsync()
        {
            checkUser();
            if (loginflag == true)
            {
                var loginuser = await _userManager.GetUserAsync(User);
                GroupedCustDept model = new GroupedCustDept();

                if (loginuser.UserRole == "RMB Super Admin")
                {
                    model.CustomerList = new SelectList(_db.Customers, "CustomerCode", "CustomerCode");
                }
                else
                {
                    var customerDb = _db.Customers.Where(x => x.CustomerCode == loginuser.CustomerCode).FirstOrDefault();
                    if (String.IsNullOrEmpty(loginuser.DepartmentPermission))
                    {
                        model.DepartmentList = new SelectList(_db.Departments.Where(y => y.CustomerCode == loginuser.CustomerCode).ToList(), "DepartmentCode", "DepartmentCode");
                        model.PerDeptList = new SelectList(_db.Departments.Where(y => y.CustomerCode == loginuser.CustomerCode).ToList(), "DepartmentCode", "DepartmentCode");
                    }
                    else
                    {
                        string PerDeptList = loginuser.DepartmentPermission;
                        char[] separators = new char[] { ' ', ',' };
                        string[] subs = PerDeptList.Split(separators, StringSplitOptions.RemoveEmptyEntries);
                        model.DepartmentList = new SelectList(subs);
                        model.PerDeptList = new SelectList(subs);
                    }
                    if (customerDb == null)
                    {
                        return NotFound();
                    }
                    model.Customer = customerDb;
                }
                return View(model);
            }
            else
            {
                return Redirect("~/Identity/Account/Login/");
            }
        }
        public async Task<IActionResult> FCollectNewRecord(GroupedCustDept model)
        {
            checkUser();
            if (loginflag == true)
            {
                var loginuser = await _userManager.GetUserAsync(User);
                ModelState.Remove("Customer");
                ModelState.Remove("OrderList");
                ModelState.Remove("DepartmentList");
                ModelState.Remove("CustomerList");
                ModelState.Remove("OrderRequest.UserId");
                ModelState.Remove("OrderRequest.Remark");
                ModelState.Remove("OrderRequest.TieQty");
                ModelState.Remove("OrderRequest.RICQty");
                ModelState.Remove("OrderRequest.PlasticBagQty");
                ModelState.Remove("OrderRequest.TamperSealQty");
                ModelState.Remove("OrderRequest.OrderId");
                ModelState.Remove("OrderRequest.OrderStatus");
                ModelState.Remove("OrderRequest.OrderType");
                ModelState.Remove("OrderRequest.COrdBarCodeId");
                ModelState.Remove("OrderRequest.DepartmentCode");
                ModelState.Remove("OrderDetail");
                ModelState.Remove("OrderDetailList");
                ModelState.Remove("PerDeptList");
                if (ModelState.IsValid)
                {
                    model.OrderRequest.UserId = loginuser.UserCode;
                    var lastOrderId = _db.OrderRequests.Max(x => x.OrderId);
                    model.OrderRequest.OrderId = lastOrderId + 1;
                    model.OrderRequest.DepartmentCode = Request.Form["OrderRequest.DepartmentCode"];
                    model.OrderRequest.OrderType = "2";
                    model.OrderRequest.CartonQty = 1;
                    if (orderlist != "")
                    {
                        List<RecordModel>? OrderDetailList = (List<RecordModel>?)JsonSerializer.Deserialize<IList<RecordModel>>(orderlist);
                        string OrderTable = "";
                        if (OrderDetailList != null)
                        {
                            OrderTable += "<div style='font-size: 14px'><h4>Transaction Detail</h4><h4>Collect New Record</h4><table class='nth-table table table-hover pt-3'>"
                                + "<tr><th>#</th><th>Department Code</th><th>Carton No</th><th>Destruction Date</th></tr>";
                            foreach (RecordModel od in OrderDetailList)
                            {
                                OrderDetail odetail = new OrderDetail();
                                odetail.OrderId = lastOrderId + 1;
                                odetail.DepartmentCode = od.DepartmentCode;
                                odetail.RecordNo = od.RecordNo;
                                odetail.DestructionDate = od.DestructionDate;
                                _db.OrderDetail.Add(odetail);
                                _db.SaveChanges();
                                OrderTable += "<tr><td>" + od.Id + "</td><td>" + odetail.DepartmentCode + "</td><td>"
                                + odetail.RecordNo + "</td><td></td></tr>";
                            }
                            OrderTable += "</table></div>";
                        }

                        //TempData["success"] = "Your order is being processed.Your order number is " + (lastOrderId + 1) + ".You will receive a confirmation email with all your order information soon.";

                        var cust = (from a in _db.Customers
                                    where a.CustomerCode == model.OrderRequest.CustomerCode
                                    select a).FirstOrDefault();
                        var approvesetup = (from b in _db.ApprovalSetup
                                            where b.UserId == model.OrderRequest.UserId
                                            select b).FirstOrDefault();
                        if (cust.NeedApproval == "No" || approvesetup == null)
                        {
                            model.OrderRequest.OrderStatus = "Accepted";
                            string Subject = "Mitsui-Soko RMB service order (" + model.OrderRequest.OrderId + ") is approved";
                            string Body = "<div style='font-size: 14px'><table class='nth-table table table-hover pt-3'>"
                            + "<tr><td>Order No:</td><td> " + model.OrderRequest.OrderId
                            + "</td></tr>"
                            + "<tr><td>Customer Code:</td><td> " + model.OrderRequest.CustomerCode
                            + "</td></tr>"
                            + "<tr><td>Company Name:</td><td> " + model.OrderRequest.CustomerName
                            + "</td></tr>"
                            + "<tr><td>Company Address:</td><td> " + model.OrderRequest.Address1
                            + "</td></tr>"
                            + "<tr><td></td><td> " + model.OrderRequest.Address2 + "</td></tr>"
                            + "<tr><td></td><td> " + model.OrderRequest.Address3 + "</td></tr>"
                            + "<tr><td>Postal Code:</td><td> " + model.OrderRequest.Address4
                            + "</td></tr>"
                            + "<tr><td>Department:</td><td> " + model.OrderRequest.DepartmentCode
                            + "</td></tr>"
                            + "<tr><td>Contact Person:</td><td> " + model.OrderRequest.ContactPerson
                            + "</td></tr>"
                            + "<tr><td>Contact Phone No:</td><td> " + model.OrderRequest.Telephone
                            + "</td></tr>"
                            + "<tr><td>Request Date:</td><td> " + model.OrderRequest.OrderDate
                            + "</td></tr>"
                            + "<tr><td>Remarks:</td><td> " + model.OrderRequest.Remark
                            + "</td></tr><br>"
                            + OrderTable + "<br>"
                            + "Yours, <br>"
                            + "Mitsui-Soko Records Management <br></div>"
                            + "<p style='padding: 0; font-size:11px; color:#375a7f;'>Remark: This email is auto generated by Mitsui-Soko Records Management System. " +
                            "This message including any attachments may contain confidential " +
                            "or legally privileged information. Any unauthorized use, retention, reproduction " +
                            "or disclosure is prohibited and may attract civil and criminal penalties. If this e-mail has been sent to you in error, please delete it and notify us immediately."
                            + "</p>";
                            SendEmail("chaw@mitsui-soko.com.sg", Subject, Body);
                        }
                        else
                        {
                            model.OrderRequest.OrderStatus = "Pending";
                            string host = HttpContext.Request.Host.Value;
                            var callbackUrl = $"{HttpContext.Request.Scheme}://{HttpContext.Request.Host}/Order/ApproveOrder/{model.OrderRequest.OrderId}";
                            string Subject = "Mitsui-Soko RMB service order (" + model.OrderRequest.OrderId + ") need approval";
                            string Body = "<div style='font-size: 14px'><table class='nth-table table table-hover pt-3'>"
                                    + "<tr><td>Order No:</td><td> " + model.OrderRequest.OrderId
                                    + "</td></tr>"
                                    + "<tr><td>Customer Code:</td><td> " + model.OrderRequest.CustomerCode
                                    + "</td></tr>"
                                    + "<tr><td>Company Name:</td><td> " + model.OrderRequest.CustomerName
                                    + "</td></tr>"
                                    + "<tr><td>Company Address:</td><td> " + model.OrderRequest.Address1
                                    + "</td></tr>"
                                    + "<tr><td></td><td> " + model.OrderRequest.Address2 + "</td></tr>"
                                    + "<tr><td></td><td> " + model.OrderRequest.Address3 + "</td></tr>"
                                    + "<tr><td>Postal Code:</td><td> " + model.OrderRequest.Address4
                                    + "</td></tr>"
                                    + "<tr><td>Department:</td><td> " + model.OrderRequest.DepartmentCode
                                    + "</td></tr>"
                                    + "<tr><td>Contact Person:</td><td> " + model.OrderRequest.ContactPerson
                                    + "</td></tr>"
                                    + "<tr><td>Contact Phone No:</td><td> " + model.OrderRequest.Telephone
                                    + "</td></tr>"
                                    + "<tr><td>Request Date:</td><td> " + model.OrderRequest.OrderDate
                                    + "</td></tr>"
                                    + "<tr><td>Remarks:</td><td> " + model.OrderRequest.Remark
                                    + "</td></tr><br>"
                                    + OrderTable + "<br>"
                                    + "<tr><td><a href='" + callbackUrl + "'>Approve</a> | <a href='" + callbackUrl + "'>Reject</a></td></tr>"
                                    + "<br>"
                                    + "Yours, <br>"
                                    + "Mitsui-Soko Records Management <br></div>"
                                    + "<p style='padding: 0; font-size:11px; color:#375a7f;'>Remark: This email is auto generated by Mitsui-Soko Records Management System. " +
                                    "This message including any attachments may contain confidential " +
                                    "or legally privileged information. Any unauthorized use, retention, reproduction " +
                                    "or disclosure is prohibited and may attract civil and criminal penalties. If this e-mail has been sent to you in error, please delete it and notify us immediately."
                                    + "</p>";
                            var approver = (from a in _db.AppUsers
                                            where a.UserName == approvesetup.ApproverId
                                            select a.Name).FirstOrDefault();
                            var backupapprover = (from a in _db.AppUsers
                                                  where a.UserName == approvesetup.SubApproverId
                                                  select a.Name).FirstOrDefault();
                            var approveremail = (from a in _db.AppUsers
                                                 where a.UserName == approvesetup.ApproverId
                                                 select a.Email).FirstOrDefault();
                            var backupapproveremail = (from a in _db.AppUsers
                                                       where a.UserName == approvesetup.SubApproverId
                                                       select a.Email).FirstOrDefault();
                            if (approveremail != null)
                            {
                                SendEmail(approveremail, Subject, Body);
                            }
                            else
                            {
                                SendEmail(approvesetup.Email, Subject, Body);
                            }
                        }
                        _db.OrderRequests.Add(model.OrderRequest);
                        _db.SaveChanges();
                        return RedirectToAction("Transaction");
                    }
                    else
                    {
                        TempData["error"] = "Record No. must be added.";
                        model.CustomerList = new SelectList(_db.Customers, "CustomerCode", "CustomerCode");
                        return Redirect(Request.Headers["Referer"].ToString());
                    }
                }
                return View(model);
            }
            else
            {
                return Redirect("~/Identity/Account/Login/");
            }
        }

        public async Task<IActionResult> FCollectRetrievedRecord(GroupedCustDept model)
        {
            checkUser();
            if (loginflag == true)
            {
                var loginuser = await _userManager.GetUserAsync(User);
                ModelState.Remove("Customer");
                ModelState.Remove("OrderList");
                ModelState.Remove("DepartmentList");
                ModelState.Remove("CustomerList");
                ModelState.Remove("OrderRequest.UserId");
                ModelState.Remove("OrderRequest.Remark");
                ModelState.Remove("OrderRequest.TieQty");
                ModelState.Remove("OrderRequest.RICQty");
                ModelState.Remove("OrderRequest.PlasticBagQty");
                ModelState.Remove("OrderRequest.TamperSealQty");
                ModelState.Remove("OrderRequest.OrderId");
                ModelState.Remove("OrderRequest.OrderStatus");
                ModelState.Remove("OrderRequest.OrderType");
                ModelState.Remove("OrderRequest.COrdBarCodeId");
                ModelState.Remove("OrderRequest.DepartmentCode");
                ModelState.Remove("OrderDetail");
                ModelState.Remove("OrderDetailList");
                ModelState.Remove("PerDeptList");
                if (ModelState.IsValid)
                {
                    model.OrderRequest.UserId = loginuser.UserCode;
                    var lastOrderId = _db.OrderRequests.Max(x => x.OrderId);
                    model.OrderRequest.OrderId = lastOrderId + 1;
                    model.OrderRequest.DepartmentCode = Request.Form["OrderRequest.DepartmentCode"];
                    model.OrderRequest.OrderType = "4";
                    model.OrderRequest.CartonQty = 1;
                    if (orderlist != "")
                    {
                        List<RecordModel>? OrderDetailList = (List<RecordModel>?)JsonSerializer.Deserialize<IList<RecordModel>>(orderlist);
                        string OrderTable = "";
                        //Loop and insert records.
                        if (OrderDetailList != null)
                        {
                            OrderTable += "<div style='font-size: 14px'><h4>Transaction Detail</h4><h4>Collect New Record</h4><table class='nth-table table table-hover pt-3'>"
                                + "<tr><th>#</th><th>Department Code</th><th>Carton No</th><th>Destruction Date</th></tr>";
                            foreach (RecordModel od in OrderDetailList)
                            {

                                OrderDetail odetail = new OrderDetail();
                                odetail.OrderId = lastOrderId + 1;
                                odetail.DepartmentCode = od.DepartmentCode;
                                odetail.RecordNo = od.RecordNo;
                                odetail.DestructionDate = new DateTime();
                                _db.OrderDetail.Add(odetail);
                                _db.SaveChanges();
                                OrderTable += "<tr><td>" + od.Id + "</td><td>" + odetail.DepartmentCode + "</td><td>"
                                + odetail.RecordNo + "</td><td></td></tr>";
                            }
                            OrderTable += "</table></div>";
                        }

                        //TempData["success"] = "Your order is being processed.Your order number is " + (lastOrderId + 1) + ".You will receive a confirmation email with all your order information soon.";

                        var cust = (from a in _db.Customers
                                    where a.CustomerCode == model.OrderRequest.CustomerCode
                                    select a).FirstOrDefault();
                        var approvesetup = (from b in _db.ApprovalSetup
                                            where b.UserId == model.OrderRequest.UserId
                                            select b).FirstOrDefault();
                        if (cust.NeedApproval == "No" || approvesetup == null)
                        {
                            model.OrderRequest.OrderStatus = "Accepted";
                            string Subject = "Mitsui-Soko RMB service order (" + model.OrderRequest.OrderId + ") is approved";
                            string Body = "<div style='font-size: 14px'><table class='nth-table table table-hover pt-3'>"
                            + "<tr><td>Order No:</td><td> " + model.OrderRequest.OrderId
                            + "</td></tr>"
                            + "<tr><td>Customer Code:</td><td> " + model.OrderRequest.CustomerCode
                            + "</td></tr>"
                            + "<tr><td>Company Name:</td><td> " + model.OrderRequest.CustomerName
                            + "</td></tr>"
                            + "<tr><td>Company Address:</td><td> " + model.OrderRequest.Address1
                            + "</td></tr>"
                            + "<tr><td></td><td> " + model.OrderRequest.Address2 + "</td></tr>"
                            + "<tr><td></td><td> " + model.OrderRequest.Address3 + "</td></tr>"
                            + "<tr><td>Postal Code:</td><td> " + model.OrderRequest.Address4
                            + "</td></tr>"
                            + "<tr><td>Department:</td><td> " + model.OrderRequest.DepartmentCode
                            + "</td></tr>"
                            + "<tr><td>Contact Person:</td><td> " + model.OrderRequest.ContactPerson
                            + "</td></tr>"
                            + "<tr><td>Contact Phone No:</td><td> " + model.OrderRequest.Telephone
                            + "</td></tr>"
                            + "<tr><td>Request Date:</td><td> " + model.OrderRequest.OrderDate
                            + "</td></tr>"
                            + "<tr><td>Remarks:</td><td> " + model.OrderRequest.Remark
                            + "</td></tr><br>"
                            + OrderTable + "<br>"
                            + "Yours, <br>"
                            + "Mitsui-Soko Records Management <br></div>"
                            + "<p style='padding: 0; font-size:11px; color:#375a7f;'>Remark: This email is auto generated by Mitsui-Soko Records Management System. " +
                            "This message including any attachments may contain confidential " +
                            "or legally privileged information. Any unauthorized use, retention, reproduction " +
                            "or disclosure is prohibited and may attract civil and criminal penalties. If this e-mail has been sent to you in error, please delete it and notify us immediately."
                            + "</p>";
                            SendEmail("chaw@mitsui-soko.com.sg", Subject, Body);
                        }
                        else
                        {
                            model.OrderRequest.OrderStatus = "Pending";
                            string host = HttpContext.Request.Host.Value;
                            var callbackUrl = $"{HttpContext.Request.Scheme}://{HttpContext.Request.Host}/Order/ApproveOrder/{model.OrderRequest.OrderId}";
                            string Subject = "Mitsui-Soko RMB service order (" + model.OrderRequest.OrderId + ") need approval";
                            string Body = "<div style='font-size: 14px'><table class='nth-table table table-hover pt-3'>"
                                    + "<tr><td>Order No:</td><td> " + model.OrderRequest.OrderId
                                    + "</td></tr>"
                                    + "<tr><td>Customer Code:</td><td> " + model.OrderRequest.CustomerCode
                                    + "</td></tr>"
                                    + "<tr><td>Company Name:</td><td> " + model.OrderRequest.CustomerName
                                    + "</td></tr>"
                                    + "<tr><td>Company Address:</td><td> " + model.OrderRequest.Address1
                                    + "</td></tr>"
                                    + "<tr><td></td><td> " + model.OrderRequest.Address2 + "</td></tr>"
                                    + "<tr><td></td><td> " + model.OrderRequest.Address3 + "</td></tr>"
                                    + "<tr><td>Postal Code:</td><td> " + model.OrderRequest.Address4
                                    + "</td></tr>"
                                    + "<tr><td>Department:</td><td> " + model.OrderRequest.DepartmentCode
                                    + "</td></tr>"
                                    + "<tr><td>Contact Person:</td><td> " + model.OrderRequest.ContactPerson
                                    + "</td></tr>"
                                    + "<tr><td>Contact Phone No:</td><td> " + model.OrderRequest.Telephone
                                    + "</td></tr>"
                                    + "<tr><td>Request Date:</td><td> " + model.OrderRequest.OrderDate
                                    + "</td></tr>"
                                    + "<tr><td>Remarks:</td><td> " + model.OrderRequest.Remark
                                    + "</td></tr><br>"
                                    + OrderTable + "<br>"
                                    + "<tr><td><a href='" + callbackUrl + "'>Approve</a> | <a href='" + callbackUrl + "'>Reject</a></td></tr>"
                                    + "<br>"
                                    + "Yours, <br>"
                                    + "Mitsui-Soko Records Management <br></div>"
                                    + "<p style='padding: 0; font-size:11px; color:#375a7f;'>Remark: This email is auto generated by Mitsui-Soko Records Management System. " +
                                    "This message including any attachments may contain confidential " +
                                    "or legally privileged information. Any unauthorized use, retention, reproduction " +
                                    "or disclosure is prohibited and may attract civil and criminal penalties. If this e-mail has been sent to you in error, please delete it and notify us immediately."
                                    + "</p>";
                            var approver = (from a in _db.AppUsers
                                            where a.UserName == approvesetup.ApproverId
                                            select a.Name).FirstOrDefault();
                            var backupapprover = (from a in _db.AppUsers
                                                  where a.UserName == approvesetup.SubApproverId
                                                  select a.Name).FirstOrDefault();
                            var approveremail = (from a in _db.AppUsers
                                                 where a.UserName == approvesetup.ApproverId
                                                 select a.Email).FirstOrDefault();
                            var backupapproveremail = (from a in _db.AppUsers
                                                       where a.UserName == approvesetup.SubApproverId
                                                       select a.Email).FirstOrDefault();
                            if (approveremail != null)
                            {
                                SendEmail(approveremail, Subject, Body);
                            }
                            else
                            {
                                SendEmail(approvesetup.Email, Subject, Body);
                            }
                        }
                        _db.OrderRequests.Add(model.OrderRequest);
                        _db.SaveChanges();
                        return RedirectToAction("Transaction");
                    }
                    else
                    {
                        TempData["error"] = "Record No. must be added.";
                        model.CustomerList = new SelectList(_db.Customers, "CustomerCode", "CustomerCode");
                        return Redirect(Request.Headers["Referer"].ToString());
                    }
                }
                return View(model);
            }
            else
            {
                return Redirect("~/Identity/Account/Login/");
            }
        }

        public JsonResult JobInfoByCustAndDept(string custcode, string deptcode, int page)
        {
            if (deptcode == null)
            {
                if (custcode == "0")
                {
                    int recsCount = _db.Job.Where(x => x.DeleteFlag == 0).Count();
                    const int pageSize = 10;
                    if (page < 1)
                        page = 1;

                    var pager = new Pager(recsCount, page, pageSize);
                    int recSkip = (page - 1) * pageSize;
                    IEnumerable<Job> JobList = (from x in _db.Job
                                                where x.DeleteFlag == 0
                                                orderby x.OldJobNo descending
                                                select x).Skip(recSkip).Take(pager.PageSize).ToList();
                    this.ViewBag.Pager = pager;
                    //_db.Job.ToList();
                    return Json(JobList);
                }
                else
                {
                    int recsCount = _db.Job.Where(x => x.CustCode == custcode && x.DeleteFlag == 0).Count();
                    const int pageSize = 10;
                    if (page < 1)
                        page = 1;

                    var pager = new Pager(recsCount, page, pageSize);
                    int recSkip = (page - 1) * pageSize;
                    IEnumerable<Job> JobList = (from y in _db.Job
                                                where y.CustCode == custcode
                                                && y.DeleteFlag == 0
                                                orderby y.OldJobNo descending
                                                select y).Skip(recSkip).Take(pager.PageSize).ToList();
                    this.ViewBag.Pager = pager;
                    //_db.Job.Where(y => y.CustCode == custcode).ToList();
                    return Json(JobList);
                }

            }
            else
            {
                int recsCount = _db.Job.Where(x => x.CustCode == custcode && x.DeptCode == deptcode && x.DeleteFlag == 0).Count();
                const int pageSize = 10;
                if (page < 1)
                    page = 1;

                var pager = new Pager(recsCount, page, pageSize);
                int recSkip = (page - 1) * pageSize;
                IEnumerable<Job> JobList = (from y in _db.Job
                                            where y.CustCode == custcode
                                            && y.DeptCode == deptcode
                                            && y.DeleteFlag == 0
                                            orderby y.OldJobNo descending
                                            select y).Skip(recSkip).Take(pager.PageSize).ToList();
                this.ViewBag.Pager = pager;
                //_db.Job.Where(y => y.CustCode == custcode && y.DeptCode == deptcode).ToList();
                return Json(JobList);
            }
        }

        public JsonResult JobLocInfoByCustAndDept(string custcode, string deptcode, int page)
        {
            if (deptcode == null)
            {
                if (custcode == "0")
                {
                    int recsCount = _db.JobsDetLoc.Where(x => x.FileNo != "" && x.Location != "").Count();
                    const int pageSize = 10;
                    if (page < 1)
                        page = 1;

                    var pager = new Pager(recsCount, page, pageSize);
                    int recSkip = (page - 1) * pageSize;

                    IEnumerable<JobsDetLoc> JobList = (from y in _db.JobsDetLoc
                                                       where y.Location != ""
                                                       && y.FileNo != ""
                                                       orderby y.JobNo descending
                                                       select y).Skip(recSkip).Take(pager.PageSize).ToList();
                    this.ViewBag.Pager = pager;
                    // _db.JobsDetLoc.ToList();
                    return Json(JobList);
                }
                else
                {
                    int recsCount = _db.JobsDetLoc.Where(x => x.CustCode == custcode && x.FileNo != "" && x.Location != "").Count();
                    const int pageSize = 10;
                    if (page < 1)
                        page = 1;

                    var pager = new Pager(recsCount, page, pageSize);
                    int recSkip = (page - 1) * pageSize;

                    IEnumerable<JobsDetLoc> JobList = (from y in _db.JobsDetLoc
                                                       where y.CustCode == custcode
                                                       && y.Location != ""
                                                       && y.FileNo != ""
                                                       orderby y.JobNo descending
                                                       select y).Skip(recSkip).Take(pager.PageSize).ToList();
                    this.ViewBag.Pager = pager;
                    // _db.JobsDetLoc.Where(y => y.CustCode == custcode).ToList();
                    return Json(JobList);
                }

            }
            else
            {
                int recsCount = _db.JobsDetLoc.Where(x => x.CustCode == custcode && x.DeptCode == deptcode && x.FileNo != "" && x.Location != "").Count();
                const int pageSize = 10;
                if (page < 1)
                    page = 1;

                var pager = new Pager(recsCount, page, pageSize);
                int recSkip = (page - 1) * pageSize;

                IEnumerable<JobsDetLoc> JobList = (from y in _db.JobsDetLoc
                                                   where y.CustCode == custcode
                                                   && y.DeptCode == deptcode
                                                   && y.Location != ""
                                                   && y.FileNo != ""
                                                   orderby y.JobNo descending
                                                   select y).Skip(recSkip).Take(pager.PageSize).ToList();
                this.ViewBag.Pager = pager;
                //IEnumerable <JobsDetLoc> JobList = _db.JobsDetLoc.Where(y => y.CustCode == custcode && y.DeptCode == deptcode).ToList();
                return Json(JobList);
            }
        }

        public JsonResult OrderInfoByCustAndDept(string custcode, string deptcode, int page)
        {
            if (deptcode == "0")
            {
                if (custcode == "0")
                {

                    int recsCount = _db.OrderRequests.Count();
                    const int pageSize = 10;
                    if (page < 1)
                        page = 1;

                    var pager = new Pager(recsCount, page, pageSize);
                    int recSkip = (page - 1) * pageSize;
                    IEnumerable<OrderRequests> OrderList = (from x in _db.OrderRequests
                                                            orderby x.OrderId descending
                                                            select x).Skip(recSkip).Take(pager.PageSize).ToList();
                    this.ViewBag.Pager = pager;
                    return Json(OrderList);

                }
                else
                {
                    int recsCount = _db.OrderRequests.Where(x => x.CustomerCode == custcode).Count();
                    const int pageSize = 10;
                    if (page < 1)
                        page = 1;

                    var pager = new Pager(recsCount, page, pageSize);
                    int recSkip = (page - 1) * pageSize;
                    IEnumerable<OrderRequests> OrderList = (from y in _db.OrderRequests
                                                            where y.CustomerCode == custcode
                                                            orderby y.OrderId descending
                                                            select y).Skip(recSkip).Take(pager.PageSize).ToList();
                    this.ViewBag.Pager = pager;
                    //_db.OrderRequests.Where(y => y.CustomerCode == custcode).ToList();
                    return Json(OrderList);
                }
            }
            else
            {
                int recsCount = _db.OrderRequests.Where(y => y.CustomerCode == custcode && y.DepartmentCode == deptcode).Count();
                const int pageSize = 10;
                if (page < 1)
                    page = 1;

                var pager = new Pager(recsCount, page, pageSize);
                int recSkip = (page - 1) * pageSize;
                IEnumerable<OrderRequests> OrderList = (from y in _db.OrderRequests
                                                        where y.CustomerCode == custcode
                                                        && y.DepartmentCode == deptcode
                                                        orderby y.OrderId descending
                                                        select y).Skip(recSkip).Take(pager.PageSize).ToList();
                this.ViewBag.Pager = pager;
                //_db.OrderRequests.Where(y => y.CustomerCode == custcode && y.DepartmentCode == deptcode).ToList();
                return Json(OrderList);
            }
        }
        public IActionResult DetailOrder(int Id)
        {
            checkUser();
            Order model = new Order();
            if (loginflag == true)
            {
                if (Id == 0)
                {
                    return NotFound();
                }
                var fromDb = _db.OrderRequests.Find(Id);
                if (fromDb == null)
                {
                    return NotFound();
                }
                model.odRequests = (from c in _db.OrderRequests
                                    where c.Id == Id
                                    select c).FirstOrDefault();
                model.listOdDetail = (from b in _db.OrderDetail
                                      where b.OrderId == fromDb.OrderId
                                      select b).ToList();

                return View(model);
            }
            else
            {
                return Redirect("~/Identity/Account/Login/");
            }
        }

        public IActionResult DetailSavedJobD(int Id)
        {
            checkUser();
            if (loginflag == true)
            {
                JobCollect model = new JobCollect();
                if (Id == 0)
                {
                    return NotFound();
                }
                var fromDb = _db.Job.Find(Id);
                if (fromDb == null)
                {
                    return NotFound();
                }
                var srvLevel = _db.JobServiceLevel.Where(y => y.SvrId == Int32.Parse(fromDb.ServLevel)).FirstOrDefault();
                if (srvLevel != null)
                    fromDb.ServLevel = srvLevel.SrvLevel;
                fromDb.DeleteFlag = 0;
                model.Job = fromDb;
                model.JobListLoc = _db.JobsDetLoc.Where(x => x.JobNo == fromDb.OldJobNo).OrderBy(y => y.Cartons).ToList();
                model.carton = _db.JobsDetLoc.Where(a => a.JobNo == fromDb.OldJobNo).Count();
                var CustomerDb = _db.Customers.Where(x => x.CustomerCode == fromDb.CustCode).FirstOrDefault();
                if (CustomerDb == null)
                {
                    return NotFound();
                }
                model.Customer = CustomerDb;
                return View(model);
            }
            else
            {
                return Redirect("~/Identity/Account/Login/");
            }
        }

        public IActionResult DetailSavedJobC(int Id)
        {
            checkUser();
            if (loginflag == true)
            {
                JobCollect model = new JobCollect();
                if (Id == 0)
                {
                    return NotFound();
                }
                var fromDb = _db.Job.Find(Id);
                if (fromDb == null)
                {
                    return NotFound();
                }
                var srvLevel = _db.JobServiceLevel.Where(y => y.SvrId == Int32.Parse(fromDb.ServLevel)).FirstOrDefault();
                if (srvLevel != null)
                    fromDb.ServLevel = srvLevel.SrvLevel;
                fromDb.DeleteFlag = 0;
                model.Job = fromDb;
                model.JobListLoc = _db.JobsDetLoc.Where(x => x.JobNo == fromDb.OldJobNo).OrderBy(y => y.Cartons).ToList();
                model.carton = _db.JobsDetLoc.Where(a => a.JobNo == fromDb.OldJobNo).Count();
                var CustomerDb = _db.Customers.Where(x => x.CustomerCode == fromDb.CustCode).FirstOrDefault();
                if (CustomerDb == null)
                {
                    return NotFound();
                }
                model.Customer = CustomerDb;
                return View(model);
            }
            else
            {
                return Redirect("~/Identity/Account/Login/");
            }
        }

        private void SendEmail(string email, string subject, string body)
        {

            string from = _config.GetValue<string>("smtp:Sender");
            string password = _config.GetValue<string>("smtp:Password");
            try
            {
                using (MailMessage mm = new MailMessage(from, email))
                {
                    mm.Subject = subject;
                    mm.Body = body;
                    mm.IsBodyHtml = true;
                    mm.Bcc.Add(_config.GetValue<string>("smtp:BCC"));
                    using (SmtpClient smtp = new SmtpClient())
                    {
                        smtp.Host = _config.GetValue<string>("smtp:Host");
                        smtp.EnableSsl = _config.GetValue<bool>("smtp:EnableSsl");
                        smtp.Port = _config.GetValue<int>("smtp:Port");
                        smtp.UseDefaultCredentials = false;
                        smtp.Credentials = new NetworkCredential(from, password);
                        smtp.Send(mm);
                    }
                }
            }
            catch (Exception ex)
            { }
        }

        public IActionResult LogScanPack(int page)
        {
            checkUser();
            if (loginflag == true)
            {
                ScanPack model = new ScanPack();

                int recsCount = _db.LogScanPack.Count();
                const int pageSize = 10;
                if (page < 1)
                    page = 1;

                var pager = new Pager(recsCount, page, pageSize);
                int recSkip = (page - 1) * pageSize;

                var OrderRequest = (from x in _db.LogScanPack
                                    orderby x.ScanDate descending
                                    select x).Skip(recSkip).Take(pager.PageSize).ToList();
                this.ViewBag.Pager = pager;
                model.ListScanPack = (List<LogScanPack>)OrderRequest;
                return View(model);
            }
            else
            {
                return Redirect("~/Identity/Account/Login/");
            }
        }

        [HttpPost]
        public async Task<IActionResult> ImportExcelFileAsync(IFormFile FormFile)
        {
            checkUser();
            if (loginflag == true)
            {
                var loginuser = await _userManager.GetUserAsync(User);
                LogScanPack lsp = new LogScanPack();
                if (FormFile != null)
                {
                    //Create a Folder.
                    string path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Uploads");
                    if (!Directory.Exists(path))
                    {
                        Directory.CreateDirectory(path);
                    }

                    //Save the uploaded Excel file.
                    string fileName = Path.GetFileName(FormFile.FileName);
                    string filePath = Path.Combine(path, fileName);
                    int line = 0;
                    using (FileStream stream = new FileStream(filePath, FileMode.Create))
                    {
                        FormFile.CopyTo(stream);
                    }
                    try
                    {
                        using (StreamReader sr = new StreamReader(filePath))
                        {
                            // Iterating the file
                            while (sr.ReadLine() != null)
                            {
                                line++;
                            }

                        }
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("The process failed: {0}", e.ToString());
                    }

                    //LogScanPack lsp = new LogScanPack();
                    lsp.FileName = fileName;
                    lsp.ScanDate = DateTime.Now;
                    lsp.StaffId = loginuser.UserCode;
                    lsp.Location = filePath;
                    lsp.TotalSData = line;
                    lsp.Remark = "";
                    _db.LogScanPack.Add(lsp);
                    _db.SaveChanges();
                }
                return RedirectToAction("LogScanPack");
            }
            else
            {
                return Redirect("~/Identity/Account/Login/");
            }
        }

        public IActionResult DetailScanPack(int Id)
        {
            checkUser();
            if (loginflag == true)
            {
                ScanDetail sd = new ScanDetail();
                if (Id != null)
                {
                    var fromDb = _db.LogScanPack.Find(Id);
                    if (fromDb == null)
                    {
                        return NotFound();
                    }
                    if (System.IO.File.Exists(fromDb.Location))
                    {
                        using (StreamReader tr = new StreamReader(fromDb.Location))
                        {
                            sd.ScanResult = tr.ReadToEnd();
                        }
                    }
                    sd.LogScanPack = fromDb;
                    return View(sd);
                }
                return NotFound();
            }
            else
            {
                return Redirect("~/Identity/Account/Login/");
            }
        }

        //POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateScan(ScanDetail model)
        {
            checkUser();
            if (loginflag == true)
            {
                var loginuser = await _userManager.GetUserAsync(User);

                var scan = _db.LogScanPack.Where(y => y.FileName == model.LogScanPack.FileName).FirstOrDefault();

                var ScanRes = model.ScanResult;
                string aLine = "";
                StringReader strReader = new StringReader(ScanRes);
                if (strReader != null)
                {
                    while (true)
                    {
                        aLine = strReader.ReadLine();
                        if (aLine != null)
                        {
                            string[] result = Regex.Split(aLine.TrimStart(), @" +");  //aLine.Split(@" +");
                            string location = result[3].Remove(result[3].Length - 10);
                            string scanstaff = result[0];
                            string custcode = result[1].Substring(0, 4);
                            string carton = result[2];
                            JobsDetLoc? selectedjob = _db.JobsDetLoc.Where(x => x.Cartons == carton && x.CustCode == custcode).FirstOrDefault();

                            LocationHistory lh = new LocationHistory();
                            if (selectedjob != null)
                            {
                                selectedjob.Location = location;
                                selectedjob.ScannerDate = scan.ScanDate;
                                selectedjob.UpdatedDate = DateTime.Now;
                                selectedjob.FileNo = model.LogScanPack.FileName;
                                selectedjob.Status = "Stored";
                                _db.JobsDetLoc.Update(selectedjob);
                                _db.SaveChanges();
                                lh.Location = location;
                                lh.CustCode = custcode;
                                lh.DeptCode = selectedjob.DeptCode;
                                lh.ScannerDate = scan.ScanDate;
                                lh.UpdatedDate = DateTime.Now;
                                lh.CreatedDate = DateTime.Now;
                                lh.FileNo = model.LogScanPack.FileName;
                                lh.Staff = scanstaff;
                                lh.Cartons = carton;
                                lh.Status = "Stored";
                                _db.LocationHistory.Add(lh);
                                _db.SaveChanges();
                            }
                        }
                        else
                        {
                            break;
                        }
                    }
                }
                return RedirectToAction("LogScanPack");
            }
            else
            {
                return Redirect("~/Identity/Account/Login/");
            }
        }

        public IActionResult Location(int page)
        {
            checkUser();
            if (loginflag == true)
            {
                LocationHisList model = new LocationHisList();

                int recsCount = _db.LogScanPack.Count();
                const int pageSize = 10;
                if (page < 1)
                    page = 1;

                var pager = new Pager(recsCount, page, pageSize);
                int recSkip = (page - 1) * pageSize;

                var OrderRequest = (from n in _db.LocationHistory
                                    group n by n.Cartons into g
                                    select g.OrderByDescending(t => t.CreatedDate).FirstOrDefault())
                                   .Skip(recSkip).Take(pager.PageSize).ToList();

                this.ViewBag.Pager = pager;
                model.ListLocHistory = (List<LocationHistory>)OrderRequest;
                return View(model);
            }
            else
            {
                return Redirect("~/Identity/Account/Login/");
            }
        }

        public IActionResult LocationHistory(int Id)
        {
            checkUser();
            if (loginflag == true)
            {
                LocationHisList sd = new LocationHisList();
                if (Id != null)
                {
                    var fromDb = _db.LocationHistory.Find(Id);
                    if (fromDb == null)
                    {
                        return NotFound();
                    }
                    else
                    {
                        var OrderRequest = (from x in _db.LocationHistory
                                            where x.Cartons == fromDb.Cartons
                                            orderby x.CreatedDate ascending
                                            select x).ToList();
                        sd.ListLocHistory = (List<LocationHistory>)OrderRequest;
                    }
                    return View(sd);
                }
                return NotFound();
            }
            else
            {
                return Redirect("~/Identity/Account/Login/");
            }
        }

        public async Task<IActionResult> InventoryList(int page, string keywords = "", string CustomerCode = "", string DepartmentCode = "")
        {
            checkUser();
            if (loginflag == true)
            {
                var loginuser = await _userManager.GetUserAsync(User);
                JobCollect model = new JobCollect();
                string userRole = loginuser.UserRole;
                if (!string.IsNullOrEmpty(keywords))
                {
                    //page = 1;
                    ViewData["keywords"] = keywords;
                    if (userRole == "RMB Super Admin")
                    {
                        int recsCount = _db.JobsDetLoc.Where(x => x.FileNo != "" && x.Location != "" && (x.JobNo.Contains(keywords) || x.Cartons.Contains(keywords))).Count();
                        const int pageSize = 10;
                        if (page < 1)
                            page = 1;

                        var pager = new Pager(recsCount, page, pageSize);
                        int recSkip = (page - 1) * pageSize;

                        var SavedJob = (from y in _db.JobsDetLoc
                                        where y.Location != ""
                                        && y.FileNo != ""
                                        && (y.JobNo.Contains(keywords) || y.Cartons.Contains(keywords))
                                        orderby y.JobNo descending
                                        select y).Skip(recSkip).Take(pager.PageSize).ToList();
                        this.ViewBag.Pager = pager;
                        //IEnumerable<Job>? SavedJob = _db.Job.OrderBy(x => x.OldJobNo).ToList();
                        model.JobListLoc = (List<JobsDetLoc>)SavedJob;
                        model.CustomerList = new SelectList(_db.Customers, "CustomerCode", "CustomerCode");
                        model.DepartmentList = new SelectList(_db.Departments.Where(y => y.CustomerCode == loginuser.CustomerCode).ToList(), "DepartmentCode", "DepartmentCode");
                        return View(model);
                    }
                    else
                    {
                        int recsCount = _db.JobsDetLoc.Where(x => x.CustCode == loginuser.CustomerCode && x.FileNo != "" && x.Location != "" && (x.JobNo.Contains(keywords) || x.Cartons.Contains(keywords))).Count();
                        const int pageSize = 10;
                        if (page < 1)
                            page = 1;

                        var pager = new Pager(recsCount, page, pageSize);
                        int recSkip = (page - 1) * pageSize;

                        var SavedJob = (from x in _db.JobsDetLoc
                                        where x.CustCode == loginuser.CustomerCode
                                        && x.Location != ""
                                        && x.FileNo != ""
                                        && (x.JobNo.Contains(keywords) || x.Cartons.Contains(keywords))
                                        orderby x.JobNo descending
                                        select x).Skip(recSkip).Take(pager.PageSize).ToList();
                        this.ViewBag.Pager = pager;
                        model.DepartmentList = new SelectList(_db.Departments.Where(y => y.CustomerCode == loginuser.CustomerCode).ToList(), "DepartmentCode", "DepartmentCode");
                        //IEnumerable<Job>? SavedJob = _db.Job.Where(x => x.CustCode == loginuser.CustomerCode).ToList();
                        model.JobListLoc = (List<JobsDetLoc>)SavedJob;
                        return View(model);
                    }
                }
                else
                {
                    ViewData["CustomerCode"] = CustomerCode;
                    ViewData["DepartmentCode"] = DepartmentCode;
                    if (userRole == "RMB Super Admin")
                    {
                        int recsCount = _db.JobsDetLoc.Where(x => x.FileNo != "" && x.Location != ""
                        && x.CustCode == ((CustomerCode == null || CustomerCode == "") ? x.CustCode : CustomerCode)
                                        && x.DeptCode == ((DepartmentCode == null || DepartmentCode == "") ? x.DeptCode : DepartmentCode)).Count();
                        const int pageSize = 10;
                        if (page < 1)
                            page = 1;

                        var pager = new Pager(recsCount, page, pageSize);
                        int recSkip = (page - 1) * pageSize;

                        var SavedJob = (from y in _db.JobsDetLoc
                                        where y.Location != ""
                                        && y.FileNo != ""
                                        && y.CustCode == ((CustomerCode == null || CustomerCode == "") ? y.CustCode : CustomerCode)
                                        && y.DeptCode == ((DepartmentCode == null || DepartmentCode == "") ? y.DeptCode : DepartmentCode)
                                        orderby y.JobNo descending
                                        select y).Skip(recSkip).Take(pager.PageSize).ToList();
                        this.ViewBag.Pager = pager;
                        //IEnumerable<Job>? SavedJob = _db.Job.OrderBy(x => x.OldJobNo).ToList();
                        model.JobListLoc = (List<JobsDetLoc>)SavedJob;
                        model.CustomerList = new SelectList(_db.Customers, "CustomerCode", "CustomerCode");
                        model.DepartmentList = new SelectList(_db.Departments.Where(y => y.CustomerCode == loginuser.CustomerCode).ToList(), "DepartmentCode", "DepartmentCode");
                        return View(model);
                    }
                    else
                    {
                        int recsCount = _db.JobsDetLoc.Where(x => x.CustCode == loginuser.CustomerCode && x.FileNo != "" && x.Location != ""
                                        && x.DeptCode == ((DepartmentCode == null || DepartmentCode == "") ? x.DeptCode : DepartmentCode)).Count();
                        const int pageSize = 10;
                        if (page < 1)
                            page = 1;

                        var pager = new Pager(recsCount, page, pageSize);
                        int recSkip = (page - 1) * pageSize;

                        var SavedJob = (from x in _db.JobsDetLoc
                                        where x.CustCode == loginuser.CustomerCode
                                        && x.Location != ""
                                        && x.FileNo != ""
                                        && x.DeptCode == ((DepartmentCode == null || DepartmentCode == "") ? x.DeptCode : DepartmentCode)
                                        orderby x.JobNo descending
                                        select x).Skip(recSkip).Take(pager.PageSize).ToList();
                        this.ViewBag.Pager = pager;
                        model.DepartmentList = new SelectList(_db.Departments.Where(y => y.CustomerCode == loginuser.CustomerCode).ToList(), "DepartmentCode", "DepartmentCode");
                        //IEnumerable<Job>? SavedJob = _db.Job.Where(x => x.CustCode == loginuser.CustomerCode).ToList();
                        model.JobListLoc = (List<JobsDetLoc>)SavedJob;
                        return View(model);
                    }
                }
            }
            else
            {
                return Redirect("~/Identity/Account/Login/");
            }
        }

        [Route("/Order/RecordContent/{recordno}")]
        public IActionResult RecordContent(string recordno)
        {
            checkUser();
            if (loginflag == true)
            {
                RecordDetails model = new RecordDetails();
                if (recordno != null)
                {
                    model.ListRecordDetail = (from x in _db.RecordDetail
                                              where x.RecordNo == recordno
                                              select x).ToList();

                    model.JobsDetail = _db.JobsDetLoc.Where(x => x.Cartons == recordno).FirstOrDefault();
                }
                return View(model);
            }
            else
            {
                return Redirect("~/Identity/Account/Login/");
            }
        }
        public IActionResult AddRecordContent(string recordno)
        {
            checkUser();
            if (loginflag == true)
            {
                RecordDetails model = new RecordDetails();
                model.CustomerList = new SelectList(_db.Customers, "CustomerCode", "CustomerCode");
                model.JobsDetail = (from x in _db.JobsDetLoc
                                    where x.Cartons == recordno
                                    select x).FirstOrDefault();
                model.rDetail = (from y in _db.RecordDetail
                                 where y.RecordNo == recordno
                                 select y).FirstOrDefault();
                return View(model);
            }
            else
            {
                return Redirect("~/Identity/Account/Login/");
            }
        }

        public async Task<IActionResult> AddRecordAsync(RecordDetails model)
        {
            checkUser();
            if (loginflag == true)
            {
                var loginuser = await _userManager.GetUserAsync(User);
                ModelState.Remove("JobsDetail");
                ModelState.Remove("ListRecordDetail");
                ModelState.Remove("rDetail.RecordNo");
                ModelState.Remove("rDetail.DepartmentCode");
                ModelState.Remove("rDetail.CustomerCode");
                if (loginuser.UserRole == "RMB Super Admin")
                {
                    model.rDetail.CustomerCode = model.JobsDetail.CustCode;
                    model.rDetail.RecordNo = model.JobsDetail.Cartons;
                    model.rDetail.DepartmentCode = model.JobsDetail.DeptCode;
                    model.rDetail.CreatedBy = loginuser.UserName;
                    model.rDetail.UpdatedBy = loginuser.UserName;
                    model.rDetail.UpdatedDate = DateTime.Now;
                    model.rDetail.CreatedDate = DateTime.Now;
                    _db.RecordDetail.Add(model.rDetail);
                    _db.SaveChanges();

                }
                else
                {
                    model.rDetail.RecordNo = model.JobsDetail.Cartons;
                    model.rDetail.CustomerCode = loginuser.CustomerCode;
                    model.rDetail.DepartmentCode = model.JobsDetail.DeptCode;
                    model.rDetail.CreatedBy = loginuser.UserName;
                    model.rDetail.UpdatedBy = loginuser.UserName;
                    model.rDetail.UpdatedDate = DateTime.Now;
                    model.rDetail.CreatedDate = DateTime.Now;
                    _db.RecordDetail.Add(model.rDetail);
                    _db.SaveChanges();
                }
                return RedirectToAction("RecordContent", new { recordno = model.JobsDetail.Cartons });
            }
            else
            {
                return Redirect("~/Identity/Account/Login/");
            }
        }
        public IActionResult EditRecordContent(int? Id)
        {
            checkUser();
            if (loginflag == true)
            {
                if (Id == null)
                {
                    return NotFound();
                }
                var fromDb = _db.RecordDetail.Find(Id);
                if (fromDb == null)
                {
                    return NotFound();
                }
                return View(fromDb);
            }
            else
            {
                return Redirect("~/Identity/Account/Login/");
            }
        }

        //POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditRecordContentAsync(RecordDetail objRecord)
        {
            checkUser();
            if (loginflag == true)
            {
                var loginuser = await _userManager.GetUserAsync(User);
                objRecord.UpdatedBy = loginuser.UserName;
                objRecord.CreatedBy = loginuser.UserName;
                objRecord.CreatedDate = objRecord.CreatedDate;
                objRecord.UpdatedDate = DateTime.Now;
                ModelState.Remove("CreatedBy");
                ModelState.Remove("UpdatedBy");
                if (ModelState.IsValid)
                {
                    _db.RecordDetail.Update(objRecord);
                    _db.SaveChanges();
                    TempData["success"] = "Record updated successfully.";
                    return RedirectToAction("RecordContent", new { recordno = objRecord.RecordNo });
                }
                return View(objRecord);
            }
            else
            {
                return Redirect("~/Identity/Account/Login/");
            }
        }
        public IActionResult DeleteRecordContent(int? Id)
        {
            checkUser();
            if (loginflag == true)
            {
                if (Id == null || Id == 0)
                {
                    return NotFound();
                }
                var fromDb = _db.RecordDetail.Find(Id);
                if (fromDb == null)
                {
                    return NotFound();
                }
                return View(fromDb);
            }
            else
            {
                return Redirect("~/Identity/Account/Login/");
            }
        }

        //POST
        [HttpPost, ActionName("DeleteRecordContent")]
        [ValidateAntiForgeryToken]
        public IActionResult DeletePostRecord(int? Id)
        {
            checkUser();
            if (loginflag == true)
            {
                var obj = _db.RecordDetail.Find(Id);
                if (Id == null || Id == 0)
                {
                    return NotFound();
                }
                _db.RecordDetail.Remove(obj);
                _db.SaveChanges();
                TempData["success"] = "Record deleted successfully.";
                return RedirectToAction("RecordContent", new { recordno = obj.RecordNo });
            }
            else
            {
                return Redirect("~/Identity/Account/Login/");
            }
        }

        [HttpPost]
        public JsonResult InventoryInfo(string custcode, string deptcode, string recordno, string fromdate, string todate, string keyword, object? inventory)
        {
            if (recordno == null && fromdate == null && todate == null && keyword == null)
            {
                inventory = (from y in _db.JobsDetLoc
                             where y.Location != ""
                             && y.FileNo != ""
                             && y.CustCode == custcode
                             && y.DeptCode == deptcode
                             orderby y.JobNo descending
                             select y).ToList();
            }
            else if (fromdate == null && todate == null && keyword == null && recordno != null)
            {
                inventory = (from y in _db.JobsDetLoc
                             where y.Location != ""
                             && y.FileNo != ""
                             && y.CustCode == custcode
                             && y.DeptCode == deptcode
                             && y.Cartons.Contains(recordno)
                             orderby y.JobNo descending
                             select y).ToList();
            }
            else if (recordno == null && fromdate == null && todate == null && keyword != null)
            {
                inventory = (from y in _db.JobsDetLoc
                             where y.Location != ""
                             && y.FileNo != ""
                             && y.CustCode == custcode
                             && y.DeptCode == deptcode
                             && (y.Cartons.Contains(keyword)
                             || y.DeptCode.Contains(keyword))
                             orderby y.JobNo descending
                             select y).ToList();
            }
            else if (recordno == null && fromdate != null && todate != null && keyword == null)
            {
                inventory = (from y in _db.JobsDetLoc
                             where y.Location != ""
                             && y.FileNo != ""
                             && y.CustCode == custcode
                             && y.DeptCode == deptcode
                             && y.ScannerDate >= DateTime.Parse(fromdate)
                             && y.ScannerDate <= DateTime.Parse(todate)
                             orderby y.JobNo descending
                             select y).ToList();
            }
            else if (recordno != null && fromdate != null && todate != null && keyword == null)
            {
                inventory = (from y in _db.JobsDetLoc
                             where y.Location != ""
                             && y.FileNo != ""
                             && y.CustCode == custcode
                             && y.DeptCode == deptcode
                             && ((y.ScannerDate >= DateTime.Parse(fromdate)
                             && y.ScannerDate <= DateTime.Parse(todate))
                             || y.Cartons.Contains(recordno))
                             orderby y.JobNo descending
                             select y).ToList();
            }
            else
            {
                inventory = (from y in _db.JobsDetLoc
                             where y.Location != ""
                             && y.FileNo != ""
                             && y.CustCode == custcode
                             && y.DeptCode == deptcode
                             && ((y.ScannerDate >= DateTime.Parse(fromdate)
                             && y.ScannerDate <= DateTime.Parse(todate))
                             || (y.Cartons.Contains(recordno)
                             || y.Cartons.Contains(keyword)
                             ))
                             orderby y.JobNo descending
                             select y).ToList();
            }
            return Json(inventory);
        }

        public IActionResult DeleteJob(int? Id)
        {
            checkUser();
            if (loginflag == true)
            {
                if (Id == null || Id == 0)
                {
                    return NotFound();
                }
                var fromDb = _db.Job.Find(Id);
                if (fromDb == null)
                {
                    return NotFound();
                }
                return View(fromDb);
            }
            else
            {
                return Redirect("~/Identity/Account/Login/");
            }
        }

        //POST
        [HttpPost, ActionName("DeleteJob")]
        [ValidateAntiForgeryToken]
        public IActionResult DeletePostJob(int? Id)
        {
            checkUser();
            if (loginflag == true)
            {
                var obj = _db.Job.Find(Id);
                if (obj == null)
                {
                    return NotFound();
                }
                obj.DeleteFlag = 1;
                _db.Job.Update(obj);
                _db.SaveChanges();
                TempData["success"] = "Job deleted successfully.";
                return RedirectToAction("JobList");
            }
            else
            {
                return Redirect("~/Identity/Account/Login/");
            }
        }

        public async Task<IActionResult> JobPermanentAsync()
        {
            checkUser();
            if (loginflag == true)
            {
                var loginuser = await _userManager.GetUserAsync(User);
                GroupedCustDept model = new GroupedCustDept();

                if (loginuser.UserRole == "RMB Super Admin")
                {
                    model.CustomerList = new SelectList(_db.Customers, "CustomerCode", "CustomerCode");
                }
                else
                {
                    var customerDb = _db.Customers.Where(x => x.CustomerCode == loginuser.CustomerCode).FirstOrDefault();
                    if (String.IsNullOrEmpty(loginuser.DepartmentPermission))
                    {
                        model.DepartmentList = new SelectList(_db.Departments.Where(y => y.CustomerCode == loginuser.CustomerCode).ToList(), "DepartmentCode", "DepartmentCode");
                        model.PerDeptList = new SelectList(_db.Departments.Where(y => y.CustomerCode == loginuser.CustomerCode).ToList(), "DepartmentCode", "DepartmentCode");
                    }
                    else
                    {
                        string PerDeptList = loginuser.DepartmentPermission;
                        char[] separators = new char[] { ' ', ',' };
                        string[] subs = PerDeptList.Split(separators, StringSplitOptions.RemoveEmptyEntries);
                        model.DepartmentList = new SelectList(subs);
                        model.PerDeptList = new SelectList(subs);
                    }

                    if (customerDb == null)
                    {
                        return NotFound();
                    }
                    model.Customer = customerDb;
                }
                return View(model);
            }
            else
            {
                return Redirect("~/Identity/Account/Login/");
            }
        }

        //POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PermanentRecordRequestAsync(GroupedCustDept model)
        {
            checkUser();
            if (loginflag == true)
            {
                var loginuser = await _userManager.GetUserAsync(User);
                ModelState.Remove("Customer");
                ModelState.Remove("OrderList");
                ModelState.Remove("DepartmentList");
                ModelState.Remove("CustomerList");
                ModelState.Remove("OrderRequest.UserId");
                ModelState.Remove("OrderRequest.Remark");
                ModelState.Remove("OrderRequest.TieQty");
                ModelState.Remove("OrderRequest.RICQty");
                ModelState.Remove("OrderRequest.PlasticBagQty");
                ModelState.Remove("OrderRequest.TamperSealQty");
                ModelState.Remove("OrderRequest.OrderId");
                ModelState.Remove("OrderRequest.OrderStatus");
                ModelState.Remove("OrderRequest.OrderType");
                ModelState.Remove("OrderRequest.COrdBarCodeId");
                ModelState.Remove("OrderRequest.DepartmentCode");
                ModelState.Remove("OrderDetail");
                ModelState.Remove("OrderDetailList");
                ModelState.Remove("PerDeptList");
                if (ModelState.IsValid)
                {
                    model.OrderRequest.UserId = loginuser.UserCode;
                    var lastOrderId = _db.OrderRequests.Max(x => x.OrderId);
                    model.OrderRequest.OrderId = lastOrderId + 1;
                    model.OrderRequest.DepartmentCode = Request.Form["OrderRequest.DepartmentCode"];
                    model.OrderRequest.OrderType = "5";
                    model.OrderRequest.CartonQty = 1;
                    if (orderlist != "")
                    {
                        List<RecordModel>? OrderDetailList = (List<RecordModel>?)JsonSerializer.Deserialize<IList<RecordModel>>(orderlist);
                        string OrderTable = "";
                        //Loop and insert records.
                        if (OrderDetailList != null)
                        {
                            OrderTable += "<div style='font-size: 14px'><h4>Transaction Detail</h4><h4>Retrieve Records</h4><table class='nth-table table table-hover pt-3'>"
                                + "<tr><th>#</th><th>Department Code</th><th>Carton No</th></tr>";
                            foreach (RecordModel od in OrderDetailList)
                            {
                                OrderDetail odetail = new OrderDetail();
                                odetail.OrderId = lastOrderId + 1;
                                odetail.DepartmentCode = od.DepartmentCode;
                                odetail.RecordNo = od.RecordNo;
                                odetail.DestructionDate = new DateTime();
                                _db.OrderDetail.Add(odetail);
                                _db.SaveChanges();
                                OrderTable += "<tr><td>" + od.Id + "</td><td>" + odetail.DepartmentCode + "</td><td>"
                                + odetail.RecordNo + "</td></tr>";
                            }
                            OrderTable += "</table></div>";
                        }

                        //TempData["success"] = "Your order is being processed.Your order number is " + (lastOrderId + 1) + ".You will receive a confirmation email with all your order information soon.";
                        var cust = (from a in _db.Customers
                                    where a.CustomerCode == model.OrderRequest.CustomerCode
                                    select a).FirstOrDefault();

                        var approvesetup = (from b in _db.ApprovalSetup
                                            where b.UserId == model.OrderRequest.UserId
                                            select b).FirstOrDefault();
                        if (cust.NeedApproval == "No" || approvesetup == null)
                        {
                            model.OrderRequest.OrderStatus = "Accepted";
                            string Subject = "Mitsui-Soko RMB service order (" + model.OrderRequest.OrderId + ") is approved";
                            string Body = "<div style='font-size: 14px'><table class='nth-table table table-hover pt-3'>"
                            + "<tr><td>Order No:</td><td> " + model.OrderRequest.OrderId
                            + "</td></tr>"
                            + "<tr><td>Customer Code:</td><td> " + model.OrderRequest.CustomerCode
                            + "</td></tr>"
                            + "<tr><td>Company Name:</td><td> " + model.OrderRequest.CustomerName
                            + "</td></tr>"
                            + "<tr><td>Company Address:</td><td> " + model.OrderRequest.Address1
                            + "</td></tr>"
                            + "<tr><td></td><td> " + model.OrderRequest.Address2 + "</td></tr>"
                            + "<tr><td></td><td> " + model.OrderRequest.Address3 + "</td></tr>"
                            + "<tr><td>Postal Code:</td><td> " + model.OrderRequest.Address4
                            + "</td></tr>"
                            + "<tr><td>Department:</td><td> " + model.OrderRequest.DepartmentCode
                            + "</td></tr>"
                            + "<tr><td>Contact Person:</td><td> " + model.OrderRequest.ContactPerson
                            + "</td></tr>"
                            + "<tr><td>Contact Phone No:</td><td> " + model.OrderRequest.Telephone
                            + "</td></tr>"
                            + "<tr><td>Request Date:</td><td> " + model.OrderRequest.OrderDate
                            + "</td></tr>"
                            + "<tr><td>Remarks:</td><td> " + model.OrderRequest.Remark
                            + "</td></tr><br>"
                            + OrderTable + "<br>"
                            + "Yours, <br>"
                            + "Mitsui-Soko Records Management <br></div>"
                            + "<p style='padding: 0; font-size:11px; color:#375a7f;'>Remark: This email is auto generated by Mitsui-Soko Records Management System. " +
                            "This message including any attachments may contain confidential " +
                            "or legally privileged information. Any unauthorized use, retention, reproduction " +
                            "or disclosure is prohibited and may attract civil and criminal penalties. If this e-mail has been sent to you in error, please delete it and notify us immediately."
                            + "</p>";
                            SendEmail("chaw@mitsui-soko.com.sg", Subject, Body);
                        }
                        else
                        {
                            model.OrderRequest.OrderStatus = "Pending";
                            string host = HttpContext.Request.Host.Value;
                            var callbackUrl = $"{HttpContext.Request.Scheme}://{HttpContext.Request.Host}/Order/ApproveOrder/{model.OrderRequest.OrderId}";
                            string Subject = "Mitsui-Soko RMB service order (" + model.OrderRequest.OrderId + ") need approval";
                            string Body = "<div style='font-size: 14px'><table class='nth-table table table-hover pt-3'>"
                                    + "<tr><td>Order No:</td><td> " + model.OrderRequest.OrderId
                                    + "</td></tr>"
                                    + "<tr><td>Customer Code:</td><td> " + model.OrderRequest.CustomerCode
                                    + "</td></tr>"
                                    + "<tr><td>Company Name:</td><td> " + model.OrderRequest.CustomerName
                                    + "</td></tr>"
                                    + "<tr><td>Company Address:</td><td> " + model.OrderRequest.Address1
                                    + "</td></tr>"
                                    + "<tr><td></td><td> " + model.OrderRequest.Address2 + "</td></tr>"
                                    + "<tr><td></td><td> " + model.OrderRequest.Address3 + "</td></tr>"
                                    + "<tr><td>Postal Code:</td><td> " + model.OrderRequest.Address4
                                    + "</td></tr>"
                                    + "<tr><td>Department:</td><td> " + model.OrderRequest.DepartmentCode
                                    + "</td></tr>"
                                    + "<tr><td>Contact Person:</td><td> " + model.OrderRequest.ContactPerson
                                    + "</td></tr>"
                                    + "<tr><td>Contact Phone No:</td><td> " + model.OrderRequest.Telephone
                                    + "</td></tr>"
                                    + "<tr><td>Request Date:</td><td> " + model.OrderRequest.OrderDate
                                    + "</td></tr>"
                                    + "<tr><td>Remarks:</td><td> " + model.OrderRequest.Remark
                                    + "</td></tr><br>"
                                    + OrderTable + "<br>"
                                    + "<tr><td><a href='" + callbackUrl + "'>Approve</a> | <a href='" + callbackUrl + "'>Reject</a></td></tr>"
                                    + "<br>"
                                    + "Yours, <br>"
                                    + "Mitsui-Soko Records Management <br></div>"
                                    + "<p style='padding: 0; font-size:11px; color:#375a7f;'>Remark: This email is auto generated by Mitsui-Soko Records Management System. " +
                                    "This message including any attachments may contain confidential " +
                                    "or legally privileged information. Any unauthorized use, retention, reproduction " +
                                    "or disclosure is prohibited and may attract civil and criminal penalties. If this e-mail has been sent to you in error, please delete it and notify us immediately."
                                    + "</p>";

                            var approver = (from a in _db.AppUsers
                                            where a.UserName == approvesetup.ApproverId
                                            select a.Name).FirstOrDefault();
                            var backupapprover = (from a in _db.AppUsers
                                                  where a.UserName == approvesetup.SubApproverId
                                                  select a.Name).FirstOrDefault();
                            var approveremail = (from a in _db.AppUsers
                                                 where a.UserName == approvesetup.ApproverId
                                                 select a.Email).FirstOrDefault();
                            var backupapproveremail = (from a in _db.AppUsers
                                                       where a.UserName == approvesetup.SubApproverId
                                                       select a.Email).FirstOrDefault();
                            if (approveremail != null)
                            {
                                SendEmail(approveremail, Subject, Body);
                            }
                            else
                            {
                                SendEmail(approvesetup.Email, Subject, Body);
                            }
                        }
                        _db.OrderRequests.Add(model.OrderRequest);
                        _db.SaveChanges();
                        return RedirectToAction("Transaction");
                    }
                    else
                    {
                        TempData["error"] = "Record No. must be added.";
                        model.CustomerList = new SelectList(_db.Customers, "CustomerCode", "CustomerCode");
                        return Redirect(Request.Headers["Referer"].ToString());
                    }
                }
                return View(model);
            }
            else
            {
                return Redirect("~/Identity/Account/Login/");
            }
        }

        public async Task<IActionResult> JobPermanentNonAsync()
        {
            checkUser();
            if (loginflag == true)
            {
                var loginuser = await _userManager.GetUserAsync(User);
                GroupedCustDept model = new GroupedCustDept();

                if (loginuser.UserRole == "RMB Super Admin")
                {
                    model.CustomerList = new SelectList(_db.Customers, "CustomerCode", "CustomerCode");
                }
                else
                {
                    var customerDb = _db.Customers.Where(x => x.CustomerCode == loginuser.CustomerCode).FirstOrDefault();
                    if (String.IsNullOrEmpty(loginuser.DepartmentPermission))
                    {
                        model.DepartmentList = new SelectList(_db.Departments.Where(y => y.CustomerCode == loginuser.CustomerCode).ToList(), "DepartmentCode", "DepartmentCode");
                        model.PerDeptList = new SelectList(_db.Departments.Where(y => y.CustomerCode == loginuser.CustomerCode).ToList(), "DepartmentCode", "DepartmentCode");
                    }
                    else
                    {
                        string PerDeptList = loginuser.DepartmentPermission;
                        char[] separators = new char[] { ' ', ',' };
                        string[] subs = PerDeptList.Split(separators, StringSplitOptions.RemoveEmptyEntries);
                        model.DepartmentList = new SelectList(subs);
                        model.PerDeptList = new SelectList(subs);
                    }
                    if (customerDb == null)
                    {
                        return NotFound();
                    }
                    model.Customer = customerDb;
                }
                return View(model);
            }
            else
            {
                return Redirect("~/Identity/Account/Login/");
            }
        }

        //POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PermanentNonRecordRequestAsync(GroupedCustDept model)
        {
            checkUser();
            if (loginflag == true)
            {
                var loginuser = await _userManager.GetUserAsync(User);
                ModelState.Remove("Customer");
                ModelState.Remove("OrderList");
                ModelState.Remove("DepartmentList");
                ModelState.Remove("CustomerList");
                ModelState.Remove("OrderRequest.UserId");
                ModelState.Remove("OrderRequest.Remark");
                ModelState.Remove("OrderRequest.TieQty");
                ModelState.Remove("OrderRequest.RICQty");
                ModelState.Remove("OrderRequest.PlasticBagQty");
                ModelState.Remove("OrderRequest.TamperSealQty");
                ModelState.Remove("OrderRequest.OrderId");
                ModelState.Remove("OrderRequest.OrderStatus");
                ModelState.Remove("OrderRequest.OrderType");
                ModelState.Remove("OrderRequest.COrdBarCodeId");
                ModelState.Remove("OrderRequest.DepartmentCode");
                ModelState.Remove("OrderDetail");
                ModelState.Remove("OrderDetailList");
                ModelState.Remove("PerDeptList");
                if (ModelState.IsValid)
                {
                    model.OrderRequest.UserId = loginuser.UserCode;
                    var lastOrderId = _db.OrderRequests.Max(x => x.OrderId);
                    model.OrderRequest.OrderId = lastOrderId + 1;
                    model.OrderRequest.DepartmentCode = Request.Form["OrderRequest.DepartmentCode"];
                    model.OrderRequest.OrderType = "6";
                    model.OrderRequest.CartonQty = 1;
                    if (orderlist != "")
                    {
                        List<RecordModel>? OrderDetailList = (List<RecordModel>?)JsonSerializer.Deserialize<IList<RecordModel>>(orderlist);
                        string OrderTable = "";
                        //Loop and insert records.
                        if (OrderDetailList != null)
                        {
                            OrderTable += "<div style='font-size: 14px'><h4>Transaction Detail</h4><h4>Retrieve Records</h4><table class='nth-table table table-hover pt-3'>"
                                + "<tr><th>#</th><th>Department Code</th><th>Carton No</th></tr>";
                            foreach (RecordModel od in OrderDetailList)
                            {
                                OrderDetail odetail = new OrderDetail();
                                odetail.OrderId = lastOrderId + 1;
                                odetail.DepartmentCode = od.DepartmentCode;
                                odetail.RecordNo = od.RecordNo;
                                odetail.DestructionDate = new DateTime();
                                _db.OrderDetail.Add(odetail);
                                _db.SaveChanges();
                                OrderTable += "<tr><td>" + od.Id + "</td><td>" + odetail.DepartmentCode + "</td><td>"
                                + odetail.RecordNo + "</td></tr>";
                            }
                            OrderTable += "</table></div>";
                        }

                        //TempData["success"] = "Your order is being processed.Your order number is " + (lastOrderId + 1) + ".You will receive a confirmation email with all your order information soon.";
                        var cust = (from a in _db.Customers
                                    where a.CustomerCode == model.OrderRequest.CustomerCode
                                    select a).FirstOrDefault();

                        var approvesetup = (from b in _db.ApprovalSetup
                                            where b.UserId == model.OrderRequest.UserId
                                            select b).FirstOrDefault();
                        if (cust.NeedApproval == "No" || approvesetup == null)
                        {
                            model.OrderRequest.OrderStatus = "Accepted";
                            string Subject = "Mitsui-Soko RMB service order (" + model.OrderRequest.OrderId + ") is approved";
                            string Body = "<div style='font-size: 14px'><table class='nth-table table table-hover pt-3'>"
                            + "<tr><td>Order No:</td><td> " + model.OrderRequest.OrderId
                            + "</td></tr>"
                            + "<tr><td>Customer Code:</td><td> " + model.OrderRequest.CustomerCode
                            + "</td></tr>"
                            + "<tr><td>Company Name:</td><td> " + model.OrderRequest.CustomerName
                            + "</td></tr>"
                            + "<tr><td>Company Address:</td><td> " + model.OrderRequest.Address1
                            + "</td></tr>"
                            + "<tr><td></td><td> " + model.OrderRequest.Address2 + "</td></tr>"
                            + "<tr><td></td><td> " + model.OrderRequest.Address3 + "</td></tr>"
                            + "<tr><td>Postal Code:</td><td> " + model.OrderRequest.Address4
                            + "</td></tr>"
                            + "<tr><td>Department:</td><td> " + model.OrderRequest.DepartmentCode
                            + "</td></tr>"
                            + "<tr><td>Contact Person:</td><td> " + model.OrderRequest.ContactPerson
                            + "</td></tr>"
                            + "<tr><td>Contact Phone No:</td><td> " + model.OrderRequest.Telephone
                            + "</td></tr>"
                            + "<tr><td>Request Date:</td><td> " + model.OrderRequest.OrderDate
                            + "</td></tr>"
                            + "<tr><td>Remarks:</td><td> " + model.OrderRequest.Remark
                            + "</td></tr><br>"
                            + OrderTable + "<br>"
                            + "Yours, <br>"
                            + "Mitsui-Soko Records Management <br></div>"
                            + "<p style='padding: 0; font-size:11px; color:#375a7f;'>Remark: This email is auto generated by Mitsui-Soko Records Management System. " +
                            "This message including any attachments may contain confidential " +
                            "or legally privileged information. Any unauthorized use, retention, reproduction " +
                            "or disclosure is prohibited and may attract civil and criminal penalties. If this e-mail has been sent to you in error, please delete it and notify us immediately."
                            + "</p>";
                            SendEmail("chaw@mitsui-soko.com.sg", Subject, Body);
                        }
                        else
                        {
                            model.OrderRequest.OrderStatus = "Pending";
                            string host = HttpContext.Request.Host.Value;
                            var callbackUrl = $"{HttpContext.Request.Scheme}://{HttpContext.Request.Host}/Order/ApproveOrder/{model.OrderRequest.OrderId}";
                            string Subject = "Mitsui-Soko RMB service order (" + model.OrderRequest.OrderId + ") need approval";
                            string Body = "<div style='font-size: 14px'><table class='nth-table table table-hover pt-3'>"
                                    + "<tr><td>Order No:</td><td> " + model.OrderRequest.OrderId
                                    + "</td></tr>"
                                    + "<tr><td>Customer Code:</td><td> " + model.OrderRequest.CustomerCode
                                    + "</td></tr>"
                                    + "<tr><td>Company Name:</td><td> " + model.OrderRequest.CustomerName
                                    + "</td></tr>"
                                    + "<tr><td>Company Address:</td><td> " + model.OrderRequest.Address1
                                    + "</td></tr>"
                                    + "<tr><td></td><td> " + model.OrderRequest.Address2 + "</td></tr>"
                                    + "<tr><td></td><td> " + model.OrderRequest.Address3 + "</td></tr>"
                                    + "<tr><td>Postal Code:</td><td> " + model.OrderRequest.Address4
                                    + "</td></tr>"
                                    + "<tr><td>Department:</td><td> " + model.OrderRequest.DepartmentCode
                                    + "</td></tr>"
                                    + "<tr><td>Contact Person:</td><td> " + model.OrderRequest.ContactPerson
                                    + "</td></tr>"
                                    + "<tr><td>Contact Phone No:</td><td> " + model.OrderRequest.Telephone
                                    + "</td></tr>"
                                    + "<tr><td>Request Date:</td><td> " + model.OrderRequest.OrderDate
                                    + "</td></tr>"
                                    + "<tr><td>Remarks:</td><td> " + model.OrderRequest.Remark
                                    + "</td></tr><br>"
                                    + OrderTable + "<br>"
                                    + "<tr><td><a href='" + callbackUrl + "'>Approve</a> | <a href='" + callbackUrl + "'>Reject</a></td></tr>"
                                    + "<br>"
                                    + "Yours, <br>"
                                    + "Mitsui-Soko Records Management <br></div>"
                                    + "<p style='padding: 0; font-size:11px; color:#375a7f;'>Remark: This email is auto generated by Mitsui-Soko Records Management System. " +
                                    "This message including any attachments may contain confidential " +
                                    "or legally privileged information. Any unauthorized use, retention, reproduction " +
                                    "or disclosure is prohibited and may attract civil and criminal penalties. If this e-mail has been sent to you in error, please delete it and notify us immediately."
                                    + "</p>";
                            var approver = (from a in _db.AppUsers
                                            where a.UserName == approvesetup.ApproverId
                                            select a.Name).FirstOrDefault();
                            var backupapprover = (from a in _db.AppUsers
                                                  where a.UserName == approvesetup.SubApproverId
                                                  select a.Name).FirstOrDefault();
                            var approveremail = (from a in _db.AppUsers
                                                 where a.UserName == approvesetup.ApproverId
                                                 select a.Email).FirstOrDefault();
                            var backupapproveremail = (from a in _db.AppUsers
                                                       where a.UserName == approvesetup.SubApproverId
                                                       select a.Email).FirstOrDefault();
                            if (approveremail != null)
                            {
                                SendEmail(approveremail, Subject, Body);
                            }
                            else
                            {
                                SendEmail(approvesetup.Email, Subject, Body);
                            }
                        }
                        _db.OrderRequests.Add(model.OrderRequest);
                        _db.SaveChanges();
                        return RedirectToAction("Transaction");
                    }
                    else
                    {
                        TempData["error"] = "Record No. must be added.";
                        model.CustomerList = new SelectList(_db.Customers, "CustomerCode", "CustomerCode");
                        return Redirect(Request.Headers["Referer"].ToString());
                    }
                }
                return View(model);
            }
            else
            {
                return Redirect("~/Identity/Account/Login/");
            }
        }

        public async Task<IActionResult> JobDestructionAsync()
        {
            checkUser();
            if (loginflag == true)
            {
                var loginuser = await _userManager.GetUserAsync(User);
                GroupedCustDept model = new GroupedCustDept();

                if (loginuser.UserRole == "RMB Super Admin")
                {
                    model.CustomerList = new SelectList(_db.Customers, "CustomerCode", "CustomerCode");
                }
                else
                {
                    var customerDb = _db.Customers.Where(x => x.CustomerCode == loginuser.CustomerCode).FirstOrDefault();
                    if (String.IsNullOrEmpty(loginuser.DepartmentPermission))
                    {
                        model.DepartmentList = new SelectList(_db.Departments.Where(y => y.CustomerCode == loginuser.CustomerCode).ToList(), "DepartmentCode", "DepartmentCode");
                        model.PerDeptList = new SelectList(_db.Departments.Where(y => y.CustomerCode == loginuser.CustomerCode).ToList(), "DepartmentCode", "DepartmentCode");
                    }
                    else
                    {
                        string PerDeptList = loginuser.DepartmentPermission;
                        char[] separators = new char[] { ' ', ',' };
                        string[] subs = PerDeptList.Split(separators, StringSplitOptions.RemoveEmptyEntries);
                        model.DepartmentList = new SelectList(subs);
                        model.PerDeptList = new SelectList(subs);
                    }

                    if (customerDb == null)
                    {
                        return NotFound();
                    }
                    model.Customer = customerDb;
                }
                return View(model);
            }
            else
            {
                return Redirect("~/Identity/Account/Login/");
            }
        }

        //POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DestructionRecordAsync(GroupedCustDept model)
        {
            checkUser();
            if (loginflag == true)
            {
                var loginuser = await _userManager.GetUserAsync(User);
                ModelState.Remove("Customer");
                ModelState.Remove("OrderList");
                ModelState.Remove("DepartmentList");
                ModelState.Remove("CustomerList");
                ModelState.Remove("OrderRequest.UserId");
                ModelState.Remove("OrderRequest.Remark");
                ModelState.Remove("OrderRequest.TieQty");
                ModelState.Remove("OrderRequest.RICQty");
                ModelState.Remove("OrderRequest.PlasticBagQty");
                ModelState.Remove("OrderRequest.TamperSealQty");
                ModelState.Remove("OrderRequest.OrderId");
                ModelState.Remove("OrderRequest.OrderStatus");
                ModelState.Remove("OrderRequest.OrderType");
                ModelState.Remove("OrderRequest.COrdBarCodeId");
                ModelState.Remove("OrderRequest.DepartmentCode");
                ModelState.Remove("OrderDetail");
                ModelState.Remove("OrderDetailList");
                ModelState.Remove("PerDeptList");
                if (loginuser.RequestDestruct == true || loginuser.UserRole == "RMB Super Admin")
                {
                    if (ModelState.IsValid)
                    {
                        model.OrderRequest.UserId = loginuser.UserCode;
                        var lastOrderId = _db.OrderRequests.Max(x => x.OrderId);
                        model.OrderRequest.OrderId = lastOrderId + 1;
                        model.OrderRequest.DepartmentCode = Request.Form["OrderRequest.DepartmentCode"];
                        model.OrderRequest.OrderType = "7";
                        model.OrderRequest.CartonQty = 1;
                        if (orderlist != "")
                        {
                            List<RecordModel>? OrderDetailList = (List<RecordModel>?)JsonSerializer.Deserialize<IList<RecordModel>>(orderlist);
                            string OrderTable = "";
                            //Loop and insert records.
                            if (OrderDetailList != null)
                            {
                                OrderTable += "<div style='font-size: 14px'><h4>Transaction Detail</h4><h4>Retrieve Records</h4><table class='nth-table table table-hover pt-3'>"
                                    + "<tr><th>#</th><th>Department Code</th><th>Carton No</th></tr>";
                                foreach (RecordModel od in OrderDetailList)
                                {
                                    OrderDetail odetail = new OrderDetail();
                                    odetail.OrderId = lastOrderId + 1;
                                    odetail.DepartmentCode = od.DepartmentCode;
                                    odetail.RecordNo = od.RecordNo;
                                    odetail.DestructionDate = new DateTime();
                                    _db.OrderDetail.Add(odetail);
                                    _db.SaveChanges();
                                    OrderTable += "<tr><td>" + od.Id + "</td><td>" + odetail.DepartmentCode + "</td><td>"
                                    + odetail.RecordNo + "</td></tr>";
                                }
                                OrderTable += "</table></div>";
                            }

                            //TempData["success"] = "Your order is being processed.Your order number is " + (lastOrderId + 1) + ".You will receive a confirmation email with all your order information soon.";
                            var cust = (from a in _db.Customers
                                        where a.CustomerCode == model.OrderRequest.CustomerCode
                                        select a).FirstOrDefault();

                            var approvesetup = (from b in _db.ApprovalSetup
                                                where b.UserId == model.OrderRequest.UserId
                                                select b).FirstOrDefault();
                            if (cust.NeedApproval == "No" || approvesetup == null)
                            {
                                model.OrderRequest.OrderStatus = "Accepted";
                                string Subject = "Mitsui-Soko RMB service order (" + model.OrderRequest.OrderId + ") is approved";
                                string Body = "<div style='font-size: 14px'><table class='nth-table table table-hover pt-3'>"
                                + "<tr><td>Order No:</td><td> " + model.OrderRequest.OrderId
                                + "</td></tr>"
                                + "<tr><td>Customer Code:</td><td> " + model.OrderRequest.CustomerCode
                                + "</td></tr>"
                                + "<tr><td>Company Name:</td><td> " + model.OrderRequest.CustomerName
                                + "</td></tr>"
                                + "<tr><td>Company Address:</td><td> " + model.OrderRequest.Address1
                                + "</td></tr>"
                                + "<tr><td></td><td> " + model.OrderRequest.Address2 + "</td></tr>"
                                + "<tr><td></td><td> " + model.OrderRequest.Address3 + "</td></tr>"
                                + "<tr><td>Postal Code:</td><td> " + model.OrderRequest.Address4
                                + "</td></tr>"
                                + "<tr><td>Department:</td><td> " + model.OrderRequest.DepartmentCode
                                + "</td></tr>"
                                + "<tr><td>Contact Person:</td><td> " + model.OrderRequest.ContactPerson
                                + "</td></tr>"
                                + "<tr><td>Contact Phone No:</td><td> " + model.OrderRequest.Telephone
                                + "</td></tr>"
                                + "<tr><td>Request Date:</td><td> " + model.OrderRequest.OrderDate
                                + "</td></tr>"
                                + "<tr><td>Remarks:</td><td> " + model.OrderRequest.Remark
                                + "</td></tr><br>"
                                + OrderTable + "<br>"
                                + "Yours, <br>"
                                + "Mitsui-Soko Records Management <br></div>"
                                + "<p style='padding: 0; font-size:11px; color:#375a7f;'>Remark: This email is auto generated by Mitsui-Soko Records Management System. " +
                                "This message including any attachments may contain confidential " +
                                "or legally privileged information. Any unauthorized use, retention, reproduction " +
                                "or disclosure is prohibited and may attract civil and criminal penalties. If this e-mail has been sent to you in error, please delete it and notify us immediately."
                                + "</p>";
                                SendEmail("chaw@mitsui-soko.com.sg", Subject, Body);
                            }
                            else
                            {
                                model.OrderRequest.OrderStatus = "Pending";
                                string host = HttpContext.Request.Host.Value;
                                var callbackUrl = $"{HttpContext.Request.Scheme}://{HttpContext.Request.Host}/Order/ApproveOrder/{model.OrderRequest.OrderId}";
                                string Subject = "Mitsui-Soko RMB service order (" + model.OrderRequest.OrderId + ") need approval";
                                string Body = "<div style='font-size: 14px'><table class='nth-table table table-hover pt-3'>"
                                        + "<tr><td>Order No:</td><td> " + model.OrderRequest.OrderId
                                        + "</td></tr>"
                                        + "<tr><td>Customer Code:</td><td> " + model.OrderRequest.CustomerCode
                                        + "</td></tr>"
                                        + "<tr><td>Company Name:</td><td> " + model.OrderRequest.CustomerName
                                        + "</td></tr>"
                                        + "<tr><td>Company Address:</td><td> " + model.OrderRequest.Address1
                                        + "</td></tr>"
                                        + "<tr><td></td><td> " + model.OrderRequest.Address2 + "</td></tr>"
                                        + "<tr><td></td><td> " + model.OrderRequest.Address3 + "</td></tr>"
                                        + "<tr><td>Postal Code:</td><td> " + model.OrderRequest.Address4
                                        + "</td></tr>"
                                        + "<tr><td>Department:</td><td> " + model.OrderRequest.DepartmentCode
                                        + "</td></tr>"
                                        + "<tr><td>Contact Person:</td><td> " + model.OrderRequest.ContactPerson
                                        + "</td></tr>"
                                        + "<tr><td>Contact Phone No:</td><td> " + model.OrderRequest.Telephone
                                        + "</td></tr>"
                                        + "<tr><td>Request Date:</td><td> " + model.OrderRequest.OrderDate
                                        + "</td></tr>"
                                        + "<tr><td>Remarks:</td><td> " + model.OrderRequest.Remark
                                        + "</td></tr><br>"
                                        + OrderTable + "<br>"
                                        + "<tr><td><a href='" + callbackUrl + "'>Approve</a> | <a href='" + callbackUrl + "'>Reject</a></td></tr>"
                                        + "<br>"
                                        + "Yours, <br>"
                                        + "Mitsui-Soko Records Management <br></div>"
                                        + "<p style='padding: 0; font-size:11px; color:#375a7f;'>Remark: This email is auto generated by Mitsui-Soko Records Management System. " +
                                        "This message including any attachments may contain confidential " +
                                        "or legally privileged information. Any unauthorized use, retention, reproduction " +
                                        "or disclosure is prohibited and may attract civil and criminal penalties. If this e-mail has been sent to you in error, please delete it and notify us immediately."
                                        + "</p>";
                                var approver = (from a in _db.AppUsers
                                                where a.UserName == approvesetup.ApproverId
                                                select a.Name).FirstOrDefault();
                                var backupapprover = (from a in _db.AppUsers
                                                      where a.UserName == approvesetup.SubApproverId
                                                      select a.Name).FirstOrDefault();
                                var approveremail = (from a in _db.AppUsers
                                                     where a.UserName == approvesetup.ApproverId
                                                     select a.Email).FirstOrDefault();
                                var backupapproveremail = (from a in _db.AppUsers
                                                           where a.UserName == approvesetup.SubApproverId
                                                           select a.Email).FirstOrDefault();
                                if (approveremail != null)
                                {
                                    SendEmail(approveremail, Subject, Body);
                                }
                                else
                                {
                                    SendEmail(approvesetup.Email, Subject, Body);
                                }
                            }
                            _db.OrderRequests.Add(model.OrderRequest);
                            _db.SaveChanges();
                            return RedirectToAction("Transaction");
                        }
                        else
                        {
                            TempData["error"] = "Record No. must be added.";
                            model.CustomerList = new SelectList(_db.Customers, "CustomerCode", "CustomerCode");
                            return Redirect(Request.Headers["Referer"].ToString());
                        }
                    }
                    return View(model);
                }
                else
                {
                    TempData["error"] = "It is not allowed to Request Destruction Service.";
                    model.CustomerList = new SelectList(_db.Customers, "CustomerCode", "CustomerCode");
                    return Redirect(Request.Headers["Referer"].ToString());
                }
            }
            else
            {
                return Redirect("~/Identity/Account/Login/");
            }
        }

        public IActionResult ApproveOrder(int Id)
        {
            checkUser();
            Approve model = new Approve();
            if (loginflag == true)
            {
                if (Id == 0)
                {
                    return NotFound();
                }
                //var fromDb = _db.OrderRequests.Where( x => x.OrderId == Id);
                //if (fromDb == null)
                //{
                //    return NotFound();
                //}
                var fromDb = (from c in _db.OrderRequests
                              where c.OrderId == Id
                              select c).FirstOrDefault();
                if (fromDb == null)
                {
                    return NotFound();
                }
                model.odRequests = fromDb;
                var approvesetup = (from b in _db.ApprovalSetup
                                    where b.UserId == fromDb.UserId
                                    select b).FirstOrDefault();
                model.username = (from a in _db.AppUsers
                                  where a.UserName == fromDb.UserId
                                  select a.Name).FirstOrDefault();
                if (approvesetup != null)
                {
                    model.approvername = (from a in _db.AppUsers
                                          where a.UserName == approvesetup.ApproverId
                                          select a.Name).FirstOrDefault();
                    model.backupapprover = (from a in _db.AppUsers
                                            where a.UserName == approvesetup.SubApproverId
                                            select a.Name).FirstOrDefault();
                    model.approveremail = (from a in _db.AppUsers
                                           where a.UserName == approvesetup.ApproverId
                                           select a.Email).FirstOrDefault();
                    model.backupapproveremail = (from a in _db.AppUsers
                                                 where a.UserName == approvesetup.SubApproverId
                                                 select a.Email).FirstOrDefault();
                    model.approveset = approvesetup;
                }
                //string Code = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(code));
                return View(model);
            }
            else
            {
                //if (code == null)
                //{
                //    return BadRequest("A code must be supplied for approve order request.");
                //}
                //else
                //{
                //    var fromDb = (from c in _db.OrderRequests
                //                  where c.OrderId == Id
                //                  select c).FirstOrDefault();
                //    if (fromDb == null)
                //    {
                //        return NotFound();
                //    }
                //    model.odRequests = fromDb;
                //    var approvesetup = (from b in _db.ApprovalSetup
                //                        where b.UserId == fromDb.UserId
                //                        select b).FirstOrDefault();
                //    model.username = (from a in _db.AppUsers
                //                      where a.UserName == fromDb.UserId
                //                      select a.Name).FirstOrDefault();
                //    if (approvesetup != null)
                //    {
                //        model.approvername = (from a in _db.AppUsers
                //                              where a.UserName == approvesetup.ApproverId
                //                              select a.Name).FirstOrDefault();
                //        model.backupapprover = (from a in _db.AppUsers
                //                                where a.UserName == approvesetup.SubApproverId
                //                                select a.Name).FirstOrDefault();
                //        model.approveremail = (from a in _db.AppUsers
                //                               where a.UserName == approvesetup.ApproverId
                //                               select a.Email).FirstOrDefault();
                //        model.backupapproveremail = (from a in _db.AppUsers
                //                                     where a.UserName == approvesetup.SubApproverId
                //                                     select a.Email).FirstOrDefault();
                //        model.approveset = approvesetup;
                //    }
                //    string Code = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(code));
                //    return View(model);
                //}
                return Redirect("~/Identity/Account/Login/");
            }
        }

        //POST
        [HttpPost]
        public async Task<IActionResult> ApproveAction(Approve model, string submit)
        {
            checkUser();
            if (loginflag == true)
            {
                var loginuser = await _userManager.GetUserAsync(User);

                var record = _db.OrderRequests.Where(a => a.OrderId == model.odRequests.OrderId).FirstOrDefault();
                if (record.OrderType == "7" && loginuser.AppDestructService == false && loginuser.UserRole != "RMB Super Admin")
                {
                    //to update
                    TempData["error"] = "It is not allowed to approve Destruction Service Request.";
                    return Redirect(Request.Headers["Referer"].ToString());
                }
                else
                {
                    var OrderDetailList = _db.OrderDetail.Where(x => x.OrderId == model.odRequests.OrderId).ToList();
                    string OrderTable = "";
                    //Loop and insert records. 
                    if (OrderDetailList != null)
                    {
                        OrderTable += "<div style='font-size: 14px'><h4>Transaction Detail</h4><h4>Retrieve Records</h4><table class='nth-table table table-hover pt-3'>"
                            + "<tr><th>#</th><th>Department Code</th><th>Carton No</th></tr>";
                        foreach (var od in OrderDetailList)
                        {
                            OrderTable += "<tr><td>" + od.Id + "</td><td>" + od.DepartmentCode + "</td><td>"
                            + od.RecordNo + "</td></tr>";
                        }
                        OrderTable += "</table></div>";
                    }
                    if (record != null)
                    {
                        string Subject = "";
                        switch (submit)
                        {
                            case "Approve Order":
                                record.OrderStatus = "Accepted";
                                _db.OrderRequests.Update(record);
                                _db.SaveChanges();
                                Subject = "Mitsui-Soko RMB service order (" + record.OrderId + ") is approved";
                                break;
                            case "Reject Order":
                                record.OrderStatus = "Rejected";
                                _db.OrderRequests.Update(record);
                                _db.SaveChanges();
                                Subject = "Mitsui-Soko RMB service order (" + record.OrderId + ") is rejected";
                                break;
                        }
                        string Body = "<div style='font-size: 14px'><table class='nth-table table table-hover pt-3'>"
                                + "<tr><td>Order No:</td><td> " + record.OrderId
                                + "</td></tr>"
                                + "<tr><td>Customer Code:</td><td> " + record.CustomerCode
                                + "</td></tr>"
                                + "<tr><td>Company Name:</td><td> " + record.CustomerName
                                + "</td></tr>"
                                + "<tr><td>Company Address:</td><td> " + record.Address1
                                + "</td></tr>"
                                + "<tr><td></td><td> " + record.Address2 + "</td></tr>"
                                + "<tr><td></td><td> " + record.Address3 + "</td></tr>"
                                + "<tr><td>Postal Code:</td><td> " + record.Address4
                                + "</td></tr>"
                                + "<tr><td>Department:</td><td> " + record.DepartmentCode
                                + "</td></tr>"
                                + "<tr><td>Contact Person:</td><td> " + record.ContactPerson
                                + "</td></tr>"
                                + "<tr><td>Contact Phone No:</td><td> " + record.Telephone
                                + "</td></tr>"
                                + "<tr><td>Request Date:</td><td> " + record.OrderDate
                                + "</td></tr>"
                                + "<tr><td>Remarks:</td><td> " + record.Remark
                                + "</td></tr><br>"
                                + OrderTable + "<br>"
                                + "Yours, <br>"
                                + "Mitsui-Soko Records Management <br></div>"
                                + "<p style='padding: 0; font-size:11px; color:#375a7f;'>Remark: This email is auto generated by Mitsui-Soko Records Management System. " +
                                "This message including any attachments may contain confidential " +
                                "or legally privileged information. Any unauthorized use, retention, reproduction " +
                                "or disclosure is prohibited and may attract civil and criminal penalties. If this e-mail has been sent to you in error, please delete it and notify us immediately."
                                + "</p>";
                        SendEmail("chaw@mitsui-soko.com.sg", Subject, Body);
                        //rmbservice@mitsui-soko.com.sg
                    }
                }
                return RedirectToAction("Transaction");
            }
            else
            {
                return Redirect("~/Identity/Account/Login/");
            }
        }
    }
}
