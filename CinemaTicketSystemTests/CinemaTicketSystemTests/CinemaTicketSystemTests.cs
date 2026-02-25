using System;
using Xunit;
using CinemaTicketSystem;

namespace CinemaTicketSystemTests
{
    public class CinemaTicketSystemTests
    {
        private readonly ITicketPriceCalculator calculator;
        private const int BasePrice = 300;

        public CinemaTicketSystemTests()
        {
            calculator = new TicketPriceCalculator();
        }

        //Базовая стоимость билета
        [Fact]
        public void CalculatePrice_NoDiscount_ReturnsBasePrice()
        {
            var request = new TicketRequest
            {
                Age = 30,
                IsStudent = false,
                IsVip = false,
                Day = DayOfWeek.Monday,
                SessionTime = TimeSpan.Parse("15:00:00")
            };

            var result = calculator.CalculatePrice(request);

            Assert.Equal(BasePrice, result);
        }

        //Бесплатные билеты для детей до 6 лет
        [Fact]
        public void CalculatePrice_AgeUnder6_ReturnsZero()
        {
            var request = new TicketRequest
            {
                Age = 5,
                IsStudent = false,
                IsVip = false,
                Day = DayOfWeek.Monday,
                SessionTime = TimeSpan.Parse("15:00:00")
            };

            var result = calculator.CalculatePrice(request);

            Assert.Equal(0, result);
        }

        //Скидка 40% для детей 6-17 лет
        [Theory]
        [InlineData(6)]
        [InlineData(10)]
        [InlineData(17)]
        public void CalculatePrice_Age6To17_Applies40PercentDiscount(int age)
        {
            var request = new TicketRequest
            {
                Age = age,
                IsStudent = false,
                IsVip = false,
                Day = DayOfWeek.Monday,
                SessionTime = TimeSpan.Parse("15:00:00")
            };

            var result = calculator.CalculatePrice(request);

            Assert.Equal(BasePrice * 0.6m, result);
        }

        //Скидка 20% для студентов 18-25 лет
        [Theory]
        [InlineData(18)]
        [InlineData(20)]
        [InlineData(25)]
        public void CalculatePrice_StudentAge18To25_Applies20PercentDiscount(int age)
        {
            var request = new TicketRequest
            {
                Age = age,
                IsStudent = true,
                IsVip = false,
                Day = DayOfWeek.Monday,
                SessionTime = TimeSpan.Parse("15:00:00")
            };

            var result = calculator.CalculatePrice(request);

            Assert.Equal(BasePrice * 0.8m, result);
        }

        //Проверяет, что студенческая скидка не применяется после 25 лет
        [Fact]
        public void CalculatePrice_StudentAgeOver25_NoDiscount()
        {
            var request = new TicketRequest
            {
                Age = 26,
                IsStudent = true,
                IsVip = false,
                Day = DayOfWeek.Monday,
                SessionTime = TimeSpan.Parse("15:00:00")
            };

            var result = calculator.CalculatePrice(request);

            Assert.Equal(BasePrice, result);
        }

        //Скидка 50% для пенсионеров 65+
        [Theory]
        [InlineData(65)]
        [InlineData(70)]
        [InlineData(100)]
        public void CalculatePrice_PensionerAge65Plus_Applies50PercentDiscount(int age)
        {
            var request = new TicketRequest
            {
                Age = age,
                IsStudent = false,
                IsVip = false,
                Day = DayOfWeek.Monday,
                SessionTime = TimeSpan.Parse("15:00:00")
            };

            var result = calculator.CalculatePrice(request);

            Assert.Equal(BasePrice * 0.5m, result);
        }

        //Утренняя скидка (до 12:00) 15%
        [Theory]
        [InlineData(9)]
        [InlineData(10)]
        [InlineData(11)]
        public void CalculatePrice_MorningSession_Applies15PercentDiscount(int hour)
        {
            var request = new TicketRequest
            {
                Age = 30,
                IsStudent = false,
                IsVip = false,
                Day = DayOfWeek.Monday,
                SessionTime = TimeSpan.FromHours(hour)
            };

            var result = calculator.CalculatePrice(request);

            Assert.Equal(BasePrice * 0.85m, result);
        }

        //Скидка в среду 30%
        [Fact]
        public void CalculatePrice_WednesdaySession_Applies30PercentDiscount()
        {
            var request = new TicketRequest
            {
                Age = 30,
                IsStudent = false,
                IsVip = false,
                Day = DayOfWeek.Wednesday,
                SessionTime = TimeSpan.Parse("15:00:00")
            };

            var result = calculator.CalculatePrice(request);

            Assert.Equal(BasePrice * 0.7m, result);
        }

        //VIP-наценка +100% применяется к итоговой цене после применения скидки
        [Fact]
        public void CalculatePrice_VipTicket_DoublesPrice()
        {
            var request = new TicketRequest
            {
                Age = 30,
                IsStudent = false,
                IsVip = true,
                Day = DayOfWeek.Monday,
                SessionTime = TimeSpan.Parse("15:00:00")
            };

            var result = calculator.CalculatePrice(request);

            Assert.Equal(BasePrice * 2m, result);
        }

        //Применяется только максимальная скидка
        [Fact]
        public void CalculatePrice_MultipleDiscounts_AppliesOnlyHighestDiscount()
        {
            var request = new TicketRequest
            {
                Age = 70,
                IsStudent = false,
                IsVip = false,
                Day = DayOfWeek.Wednesday,
                SessionTime = TimeSpan.Parse("11:00:00")
            };

            var result = calculator.CalculatePrice(request);

            Assert.Equal(BasePrice * 0.5m, result);
        }

        //VIP-наценка после применения максимальной скидки
        [Fact]
        public void CalculatePrice_VipWithMaxDiscount_AppliesVipAfterDiscount()
        {
            var request = new TicketRequest
            {
                Age = 70,
                IsStudent = false,
                IsVip = true,
                Day = DayOfWeek.Wednesday,
                SessionTime = TimeSpan.Parse("11:00:00")
            };

            var result = calculator.CalculatePrice(request);

            Assert.Equal((BasePrice * 0.5m) * 2m, result);
        }

        // Проверка граничных значений возраста
        [Theory]
        [InlineData(0, 0)]
        [InlineData(5, 0)]
        [InlineData(6, BasePrice * 0.6)]
        [InlineData(17, BasePrice * 0.6)]
        [InlineData(18, BasePrice * 0.8)]
        [InlineData(25, BasePrice * 0.8)]
        [InlineData(26, BasePrice)]
        [InlineData(64, BasePrice)]
        [InlineData(65, BasePrice * 0.5)]
        public void CalculatePrice_AgeBoundaryValues_AppliesCorrectDiscount(int age, decimal expectedPrice)
        {
            var ticket = new TicketRequest
            {
                Age = age,
                IsStudent = true,
                IsVip = false,
                Day = DayOfWeek.Friday,
                SessionTime = TimeSpan.Parse("13:10:10")
            };

            var result = calculator.CalculatePrice(ticket);

            Assert.Equal(expectedPrice, result);
        }

        // Проверка исключения ArgumentNullException
        [Fact]
        public void CalculatePrice_NullRequest_ThrowsArgumentNullException()
        {
            TicketRequest request = null;

            Assert.Throws<ArgumentNullException>(() => calculator.CalculatePrice(request));
        }

        //Проверка исключения для возраста < 0
        [Theory]
        [InlineData(-1)]
        [InlineData(-5)]
        [InlineData(-100)]
        public void CalculatePrice_NegativeAge_ThrowsArgumentOutOfRangeException(int age)
        {
            var request = new TicketRequest
            {
                Age = age,
                IsStudent = false,
                IsVip = false,
                Day = DayOfWeek.Monday,
                SessionTime = TimeSpan.Parse("15:00:00")
            };

            Assert.Throws<ArgumentOutOfRangeException>(() => calculator.CalculatePrice(request));
        }

        //Проверка исключения для возраста > 120
        [Theory]
        [InlineData(121)]
        [InlineData(150)]
        [InlineData(200)]
        public void CalculatePrice_AgeOver120_ThrowsArgumentOutOfRangeException(int age)
        {
            var request = new TicketRequest
            {
                Age = age,
                IsStudent = false,
                IsVip = false,
                Day = DayOfWeek.Monday,
                SessionTime = TimeSpan.Parse("15:00:00")
            };

            Assert.Throws<ArgumentOutOfRangeException>(() => calculator.CalculatePrice(request));
        }

        //Проверка VIP с комбинацией скидок
        [Fact]
        public void CalculatePrice_VipWithWednesdayAndMorningDiscounts_AppliesCorrectPrice()
        {
            var request = new TicketRequest
            {
                Age = 30,
                IsStudent = false,
                IsVip = true,
                Day = DayOfWeek.Wednesday,
                SessionTime = TimeSpan.Parse("9:00:00")
            };

            var result = calculator.CalculatePrice(request);

            Assert.Equal((BasePrice * 0.7m) * 2m, result);
        }
    }
}