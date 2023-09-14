using Forum.Data;
using Forum.Models;
using Microsoft.AspNetCore.Mvc;

namespace Forum.Controllers

{
    public class AdminController : Controller

    {

        private readonly ApplicationDbContext _context;

        public AdminController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            // Lấy danh sách các câu hỏi và câu trả lời chưa được duyệt
            var unapprovedQuestions = _context.Questions.Where(q => !q.IsApproved).ToList();
            var unapprovedAnswers = _context.Answers.Where(a => !a.IsApproved).ToList();

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
