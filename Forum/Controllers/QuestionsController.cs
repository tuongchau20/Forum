﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Forum.Data;
using Forum.Models;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;

namespace Forum.Controllers
{
    public class QuestionsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public QuestionsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Questions
        [AllowAnonymous]
        public async Task<IActionResult> Index()
        {
            var approvedQuestions = await _context.Questions
                .Where(q => q.IsApproved) // Chỉ lấy các câu hỏi đã được duyệt
                .Include(q => q.User)
                .Include(a => a.Answers.Where(ans => ans.IsApproved)) // Chỉ lấy các câu trả lời đã được duyệt
                .ToListAsync();
            return View(approvedQuestions);
        }
        // GET: Questions/Details/5
            [AllowAnynomous]
              public async Task<IActionResult> Details(int? id)
            {
                if (id == null || _context.Questions == null)
                {
                    return NotFound();
                }

                var question = await _context.Questions
                    .Include(q => q.User) 
                    .Include(c => c.Answers)
                    .Include(c => c.Answers.Where(a => a.IsApproved)) // Chỉ lấy các câu trả lời đã được duyệt
                    .ThenInclude(q => q.User)
                    .FirstOrDefaultAsync(m => m.Id == id);
                if (question == null)
                {
                    return NotFound();
                }

                return View(question);
            }

        // GET: Questions/Create
        [Authorize]
        public IActionResult Create()
        {
            return View();
        }

        // POST: Questions/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Title,Description,IdentityUserId")] Question question)
        {
            if (ModelState.IsValid)
            {
                question.IsApproved = false; // Đánh dấu câu hỏi chưa được duyệt
                _context.Add(question);
                await _context.SaveChangesAsync();
                return Content("Câu hỏi của bạn đã được gửi và đang chờ được duyệt.");
            }
            ViewData["IdentityUserId"] = new SelectList(_context.Users, "Id", "Id", question.IdentityUserId);
            return View(question);
        }
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddAnswer([Bind("Id,Content, QuestionId, IdentityUserId")] Answer answer)
        {
            if (ModelState.IsValid)
            {
                answer.IsApproved = false;
                _context.Add(answer);
                await _context.SaveChangesAsync();
            }
            var question = await _context.Questions
                .Include(q => q.User)
                .Include(c => c.Answers.Where(a => a.IsApproved)) // Chỉ lấy các câu trả lời đã được duyệt
                .ThenInclude(q => q.User)
                .FirstOrDefaultAsync(q => q.Id == answer.QuestionId);

            if (question == null)
            {
                return NotFound();
            }

            return View("Details", question);
        }
        // GET: Questions/Edit/5
        [Authorize]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Questions == null)
            {
                return NotFound();
            }

            var question = await _context.Questions.FindAsync(id);
            if (question.IdentityUserId != User.FindFirstValue(ClaimTypes.NameIdentifier))
            {
                return NotFound();
            }
            ViewData["IdentityUserId"] = new SelectList(_context.Users, "Id", "Id", question.IdentityUserId);
            return View(question);
        }

        // POST: Questions/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Title,Description,IdentityUserId")] Question question)
        {
            if (id != question.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(question);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!QuestionExists(question.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }

            return View(question);
        }

        // GET: Questions/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Questions == null)
            {
                return NotFound();
            }

            var question = await _context.Questions
                .Include(q => q.User)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (question.IdentityUserId != User.FindFirstValue(ClaimTypes.NameIdentifier))
            {
                return NotFound();
            }

            return View(question);
        }

        // POST: Questions/Delete/5
        [HttpPost, ActionName("Delete")]
        
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.Questions == null)
            {
                return Problem("Entity set 'ApplicationDbContext.Questions'  is null.");
            }
            var question = await _context.Questions.FindAsync(id);
            if (question != null)
            {
                _context.Questions.Remove(question);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool QuestionExists(int id)
        {
          return (_context.Questions?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
