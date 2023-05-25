using ExpenseTracker.Models;
using Microsoft.AspNetCore.Mvc;

namespace ExpenseTracker.Controllers
{
    public class DashboardController : Controller
    {
        private readonly ApplicationDbContext _context;
        public DashboardController(ApplicationDbContext context)
        {
            _context = context;
        }
        public async Task<IActionResult> Index()
        {
            DateTime StartDate = DateTime.Today.AddDays(-6);
            DateTime EndDate = DateTime.Today;
            List<Transaction>? selectedTransactions = await _context.Transactions.Include(e => e.Category).Where(e => e.Date >= StartDate && e.Date <= EndDate).ToListAsync();
            int TotalIncome = selectedTransactions.Where(e => e.Category.Type == "Income").Sum(e => e.Amount);
            ViewBag.TotalIncome = TotalIncome.ToString("C0");
            int TotalExpense = selectedTransactions.Where(e => e.Category.Type == "Expense").Sum(e => e.Amount);
            ViewBag.TotalExpense = TotalExpense.ToString("C0");
            int balance = TotalIncome - TotalExpense;
            ViewBag.Balance = balance.ToString("C0");

            /// Doughnut chart expense
            ViewBag.DoughnutChartData = selectedTransactions
                .Where(e => e.Category?.Type == "Expense")
                .GroupBy(e => e.Category?.CategoryId)
                .Select(k => new
                {
                    categoryTitleWithIcon = $"{k.First().Category?.Icon} {k.First().Category?.Title}" ?? "",
                    amount = k.Sum(e => e.Amount),
                    formattedAmount = k.Sum(e => e.Amount).ToString("C0"),
                }).ToList();
            // spline 
            //income
            List<SplineChartData> incomeSummary = selectedTransactions.Where(e => e.Category.Type == "Income").GroupBy(e => e.Date).Select(e => new SplineChartData
            {
                Day = e.First().Date.ToString("dd-MMM"),
                Income = e.Sum(e => e.Amount)
            }).ToList();

            //expense
            List<SplineChartData> expenseSummary = selectedTransactions.Where(e => e.Category.Type == "Expense").GroupBy(e => e.Date).Select(e => new SplineChartData
            {
                Day = e.First().Date.ToString("dd-MMM"),
                Expense = e.Sum(e => e.Amount)
            }).ToList();

            // combine income & expense  
            string[] last7Days = Enumerable.Range(0, 7).Select(i => StartDate.AddDays(i).ToString("dd-MMM")).ToArray();
            ViewBag.SplineChartData = from Day in last7Days
                                      join income in incomeSummary on Day equals income.Day into dayIncomeJoined
                                      from income in dayIncomeJoined.DefaultIfEmpty()
                                      join expense in expenseSummary on Day equals expense.Day into expenseJoined
                                      from expense in expenseJoined.DefaultIfEmpty()
                                      select new { 
                                        Day=Day,
                                        Income=income==null ? 0:income.Income,
                                        Expense = expense == null ? 0:expense.Expense,
                                      };


            return View();
        }
    }

    public class SplineChartData {
        public string? Day;
        public int Income;
        public int Expense;
    }

}
