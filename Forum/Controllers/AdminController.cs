using Forum.Data;
using Forum.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Forum.Controllers

{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller

    {

        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        public AdminController(ApplicationDbContext context, UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
        }
        public async Task<IActionResult> CreateAdmin()
        {
            var adminEmail = "admin1@gmail.com";
            var adminPassword = "admin123"; // Đặt mật khẩu cho admin

            var adminUser = new IdentityUser
            {
                UserName = adminEmail,
                Email = adminEmail,
                EmailConfirmed = true // Đảm bảo xác thực email 
            };

            var result = await _userManager.CreateAsync(adminUser, adminPassword);
            if (result.Succeeded)
            {
                // Tạo và gán vai trò Admin cho tài khoản
                var adminRole = "Admin";
                if (!await _roleManager.RoleExistsAsync(adminRole))
                {
                    await _roleManager.CreateAsync(new IdentityRole(adminRole));
                }

                await _userManager.AddToRoleAsync(adminUser, adminRole);


                return RedirectToAction("Index"); // Hoặc trả về một trang thông báo thành công
            }
            else
            {
                // Xử lý lỗi nếu cần
                return View("Error"); // Hoặc trả về trang lỗi
            }
        }
        public IActionResult Index()
        {
            // Lấy danh sách các câu hỏi và câu trả lời chưa được duyệt
            var unapprovedQuestions = _context.Questions.Where(q => !q.IsApproved).ToList();
            var unapprovedAnswers = _context.Answers.Include(a => a.Question).Where(a => !a.IsApproved).ToList();

            var model = new AdminViewModel
            {
                UnapprovedQuestions = unapprovedQuestions,
                UnapprovedAnswers = unapprovedAnswers
            };

            return View(model);
        }

        [HttpPost]
        public IActionResult ApproveQuestion(int questionId)
        {
            var question = _context.Questions.FirstOrDefault(q => q.Id == questionId);
            if (question != null)
            {
                question.IsApproved = true;
                _context.SaveChanges();
            }
            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult ApproveAnswer(int answerId)
        {
            var answer = _context.Answers.FirstOrDefault(a => a.Id == answerId);
            if (answer != null)
            {
                answer.IsApproved = true;
                _context.SaveChanges();
            }
            return RedirectToAction("Index");
        }
    }
    public class AdminViewModel
    {
        public List<Question>? UnapprovedQuestions { get; set; }
        public List<Answer>? UnapprovedAnswers { get; set; }
    }
    
}
