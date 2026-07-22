using FundWise.Domain.Entities;
using FundWise.Domain.Enums;
using FundWise.Persistence.DbContext;

namespace FundWise.Persistence.SeedData;

public static class AlternativeProductSeeder
{
    public static async Task SeedAsync(FundWiseDbContext context)
    {
        if (context.AlternativeProducts.Any()) return;

        var products = new List<AlternativeProduct>
        {
            // Groceries & Dairy
            AlternativeProduct.Create("Full Cream Milk 1L", "Juhayna", TransactionCategory.Groceries, 48.00m, "Full Cream Milk 1L", "Lamar", 40.00m, "Save 8 EGP per bottle (similar quality local dairy)"),
            AlternativeProduct.Create("Full Cream Milk 1L", "Lamar", TransactionCategory.Groceries, 40.00m, "Full Cream Milk 1L", "Bekheiro", 32.00m, "Save 8 EGP per bottle"),
            AlternativeProduct.Create("Yellow Cheese 250g", "President", TransactionCategory.Groceries, 120.00m, "Yellow Cheese 250g", "Domty", 85.00m, "Save 35 EGP per pack"),
            AlternativeProduct.Create("White Sugar 1kg", "Al Dahan", TransactionCategory.Groceries, 42.00m, "White Sugar 1kg", "El Basha", 34.00m, "Save 8 EGP per bag"),
            AlternativeProduct.Create("Sunflower Oil 1L", "Crystal", TransactionCategory.Groceries, 95.00m, "Sunflower Oil 1L", "Afia", 78.00m, "Save 17 EGP per bottle"),

            // Food & Drink
            AlternativeProduct.Create("Caffe Latte", "Starbucks", TransactionCategory.FoodAndDrink, 110.00m, "Caffe Latte", "Cilantro", 75.00m, "Save 35 EGP per coffee (Egyptian coffee chain)"),
            AlternativeProduct.Create("Americano", "Costa Coffee", TransactionCategory.FoodAndDrink, 90.00m, "Americano", "Espresso Lab", 65.00m, "Save 25 EGP per coffee"),
            AlternativeProduct.Create("Big Mac Combo", "McDonald's", TransactionCategory.FoodAndDrink, 210.00m, "Classic Burger Combo", "Buffalo Burger", 165.00m, "Save 45 EGP per meal"),
            AlternativeProduct.Create("Fried Chicken 3Pcs", "KFC", TransactionCategory.FoodAndDrink, 220.00m, "Fried Chicken 3Pcs", "Heart Attack", 170.00m, "Save 50 EGP per meal"),
            AlternativeProduct.Create("Pepperoni Pizza Large", "Pizza Hut", TransactionCategory.FoodAndDrink, 340.00m, "Pepperoni Pizza Large", "Primos Pizza", 240.00m, "Save 100 EGP per pizza"),

            // Transport
            AlternativeProduct.Create("Ride Hailing", "Uber", TransactionCategory.Transport, 120.00m, "Ride Hailing", "inDrive", 85.00m, "Save ~35 EGP per trip via bid pricing"),
            AlternativeProduct.Create("Ride Hailing", "Uber", TransactionCategory.Transport, 120.00m, "Bus Route", "Swvl", 45.00m, "Save 75 EGP per trip using premium bus lines"),

            // Shopping & Household
            AlternativeProduct.Create("Dishwashing Liquid 1L", "Fairy", TransactionCategory.Shopping, 75.00m, "Dishwashing Liquid 1L", "Pril", 52.00m, "Save 23 EGP per bottle"),
            AlternativeProduct.Create("Laundry Detergent 4kg", "Ariel", TransactionCategory.Shopping, 310.00m, "Laundry Detergent 4kg", "Tide", 230.00m, "Save 80 EGP per box"),
            AlternativeProduct.Create("Shampoo 400ml", "Head & Shoulders", TransactionCategory.Shopping, 140.00m, "Shampoo 400ml", "Sunsilk", 85.00m, "Save 55 EGP per bottle"),

            // Bills & Utilities
            AlternativeProduct.Create("Monthly Mobile Internet 40GB", "Vodafone", TransactionCategory.BillsAndUtilities, 280.00m, "Monthly Mobile Internet 40GB", "WE", 210.00m, "Save 70 EGP monthly on telecom provider"),
            AlternativeProduct.Create("Home Fiber Broadband 140GB", "Orange", TransactionCategory.BillsAndUtilities, 250.00m, "Home VDSL 140GB", "WE", 170.00m, "Save 80 EGP monthly on home internet")
        };

        await context.AlternativeProducts.AddRangeAsync(products);
        await context.SaveChangesAsync();
    }
}
