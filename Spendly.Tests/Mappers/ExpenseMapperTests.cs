//using Spendly.Application.Mappers;
//using Spendly.Domain.Entities;
//using Spendly.Domain.ValueObjects;
//using Xunit;

//namespace Spendly.Tests.Mappers
//{
//    public class ExpenseMapperTests
//    {
//        [Fact]
//        public void ToDto_Should_Map_Expense_To_ExpenseResponseDto_Correctly()
//        {
//            // Arrange
//            var expense = Expense.Create(
//            Money.FromDecimal(250),
//            "Lunch",
//            DateTime.Now,
//            "Food"
//            );

//            var dto = ExpenseMapper.ToDto(expense);

//            Assert.Equal(expense.Id, dto.Id);
//            Assert.Equal(250, dto.Amount);
//            Assert.Equal("Lunch", dto.Description);
//            Assert.Equal("Food", dto.Category);
//        }
//    }
//}
