using Moq;
using HealthDiary.Model;
using NUnit.Framework;
using System;

namespace HealthDiaryTests
{
    public class CalcCaloriesPlanTests
    {
        public Mock<User> user_mock = new Mock<User>();
        public Mock<Gender> gender_mock = new Mock<Gender>();
        public Mock<PhysicalActivityType> type_mock = new Mock<PhysicalActivityType>();

        [SetUp]
        public void Setup()
        { }

        [Test]
        public void Test1() //1,2,3,4,7,9,11,12
        {
            double weight = 50;
            double growth = 150;

            double gender_rate = 10;
            double type_rate = 10;
            InitMocks(gender_rate, type_rate);

            DateTime birthdate = new DateTime(2024, 1, 1);

            //double expected = (weight * 10 + growth * 6.25 - 5 * 0 + 0) * (0);
            double expected = 0;
            double result = user_mock.Object.CalculatePlanCalories(birthdate, null, null, weight, growth);

            Assert.AreEqual(result, expected);
        }

        [Test]
        public void Test2() //1,2,3,4,7,8,9,10,11,12
        {
            double weight = 50;
            double growth = 150;

            double gender_rate = 10;
            double type_rate = 10;
            InitMocks(gender_rate, type_rate);

            DateTime birthdate = new DateTime(2024, 1, 1);

            //double expected = (weight * 10 + growth * 6.25 - 5 * 0 + gender_rate) * (type_rate);
            double expected = 14475;
            double result = user_mock.Object.CalculatePlanCalories(birthdate, gender_mock.Object, type_mock.Object, weight, growth);

            Assert.AreEqual(result, expected);
        }

        [Test]
        public void Test3() //1,2,3,5',6,7,9,10,11,12
        {
            double weight = 50;
            double growth = 150;

            double gender_rate = 10;
            double type_rate = 10;
            InitMocks(gender_rate, type_rate);

            DateTime birthdate = new DateTime(1980, 1, 1);

            //double expected = (weight * 10 + growth * 6.25 - 5 * 43 + 0) * (type_rate);
            double expected = 12225;
            double result = user_mock.Object.CalculatePlanCalories(birthdate, null, type_mock.Object, weight, growth);

            Assert.AreEqual(result, expected);
        }

        [Test]
        public void Test4() //1,2,3,5',5'',7,8,9,11,12
        {
            double weight = 50;
            double growth = 150;

            double gender_rate = 10;
            double type_rate = 10;
            InitMocks(gender_rate, type_rate);

            DateTime birthdate = new DateTime(1980, 12, 1);

            //double expected = (weight * 10 + growth * 6.25 - 5 * 42 + gender_rate) * (0);
            double expected = 0;
            double result = user_mock.Object.CalculatePlanCalories(birthdate, gender_mock.Object, null, weight, growth);

            Assert.AreEqual(result, expected);
        }

        [Test]
        public void Test5() //1,2,3,5',5'',5''',6,7,8,9,10,11,12
        {
            double weight = 50;
            double growth = 150;

            double gender_rate = 10;
            double type_rate = 10;
            InitMocks(gender_rate, type_rate);

            DateTime birthdate = new DateTime(1980, 6, 16);

            //double expected = (weight * 10 + growth * 6.25 - 5 * 43 + gender_rate) * (type_rate);
            double expected = 12325;
            double result = user_mock.Object.CalculatePlanCalories(birthdate, gender_mock.Object, type_mock.Object, weight, growth);

            Assert.AreEqual(result, expected);
        }

        [Test]
        public void Test6() //1,2,3,5',5'',5''',7,9,11,12
        {
            double weight = 50;
            double growth = 150;

            double gender_rate = 10;
            double type_rate = 10;
            InitMocks(gender_rate, type_rate);

            DateTime birthdate = new DateTime(1980, 6, 29);

            //double expected = (weight * 10 + growth * 6.25 - 5 * 42 + 0) * (0);
            double expected = 0;
            double result = user_mock.Object.CalculatePlanCalories(birthdate, null, null, weight, growth);

            Assert.AreEqual(result, expected);
        }

        private void InitMocks(double gender_rate, double type_rate)
        {
            gender_mock.SetupAllProperties();
            gender_mock.Object.Rate = gender_rate;
            type_mock.SetupAllProperties();
            type_mock.Object.Rate = type_rate;
        }
    }
}