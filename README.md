# 🛒 Ecommerce Backend API (ASP.NET Core 9 + EF Core + JWT)

A clean and modular **Ecommerce Backend API** built with **ASP.NET Core 9**, **Entity Framework Core**, **SQL Server**, and **JWT Authentication**. The project follows a simple but scalable folder structure, supports core ecommerce workflows, and is suitable for real-world use or showcasing in a portfolio.

---

## 🚀 Features

* **Authentication & Authorization** using ASP.NET Identity + JWT.
* **Product Management**

  * CRUD operations
  * Search, filter, and pagination
* **Category Management**
* **Shopping Cart**

  * Add / Update / Remove items
  * Auto-create cart per user
* **Checkout & Orders**

  * Order placement
  * Order items linked to products
  * Order status enum (Pending → Paid → Shipped → Delivered → Cancelled)
* **Simple Fake Payment Flow** (sufficient for demo/portfolio)
* **DTOs separated from Entities**
* **Clean project structure**
* **Swagger UI enabled**

---

## 📁 Project Structure

```
EcommerceSolution/
│
├── Ecommerce.Api/
│   ├── Controllers/
│   ├── wwwroot/
│   ├── Properties/
│   ├── appsettings.json
│   ├── Ecommerce.Api.http
│   └── Program.cs
│
├── Ecommerce.Application/
│   ├── DTO/
│   │   ├── Auth/
│   │   ├── Cart/
│   │   ├── Category/
│   │   ├── Product/
│   │   ├── Orders/
│   ├── Validators/   (optional but recommended)
│   └── (No Services, since you chose controller-level logic)
│
├── Ecommerce.Core/
│   ├── Identity/
│   │   ├── ApplicationUser.cs
│   ├── Models/
│   │   ├── Cart.cs
│   │   ├── CartItem.cs
│   │   ├── Category.cs
│   │   ├── Product.cs
│   │   ├── ProductImage.cs
│   │   ├── Order.cs
│   │   ├── OrderItem.cs
│   │   ├── Enums/
│   │   │   └── OrderStatus.cs
│
├── Ecommerce.Infrastructure/
│   ├── Data/
│   │   ├── AppDbContext.cs
│   │   ├── DatabaseSeeder.cs
│   ├── Migrations/
│   ├── Services/
│   │   ├── LocalFileStorageService.cs
│   └── DesignTime/
│       ├── AppDbContextFactory.cs
│
└── README.md
└── DOCUMENTATION.md
```

---

## 🗄️ Database

* **SQL Server**
* Migration commands:

```
dotnet ef migrations add InitialCreate -p Ecommerce.Infrastructure -s Ecommerce.Api
dotnet ef database update -p Ecommerce.Infrastructure -s Ecommerce.Api
```

---

## 🔑 Authentication

This project uses **ASP.NET Identity** with **JWT tokens**.

* Register
* Login
* Token issuance
* Secured endpoints for Cart, Orders, Profile

---

## 📦 API Endpoints (Summary)

### **Auth**

* `POST /api/auth/register`
* `POST /api/auth/login`

### **Products**

* `GET /api/products`
* `GET /api/products/{id}`
* `POST /api/products`
* `PUT /api/products/{id}`
* `DELETE /api/products/{id}`
* `GET /api/products/search?query=`

### **Categories**

* CRUD operations

### **Cart**

* `GET /api/cart`
* `POST /api/cart/items`
* `PUT /api/cart/items/{cartItemId}`
* `DELETE /api/cart/items/{cartItemId}`

### **Orders**

* `POST /api/orders/checkout`
* `GET /api/orders/my`
* `GET /api/orders/{orderId}`

---

## 📘 Documentation

This project includes:

* Entities breakdown
* DTO summary
* Checkout workflow
* Order lifecycle
* Payment simulation explanation
* Authentication flow

(Currently contained in this README — can be expanded into a full `/docs` folder if needed.)

---

## 👨‍💻 How to Run

**1. Restore packages**

```
dotnet restore
```

**2. Apply migrations**

```
dotnet ef database update -p Ecommerce.Infrastructure -s Ecommerce.Api
```

**3. Run the API**

```
dotnet run --project Ecommerce.Api
```

Navigate to:

```
https://localhost:<port>/swagger
```

---

## 🧩 Technologies Used

* **ASP.NET Core 9**
* **Entity Framework Core**
* **SQL Server**
* **JWT Authentication**
* **ASP.NET Identity**
* **Swagger / OpenAPI**

---

## 📬 Author

**Youssef Said**
Full‑Stack Developer
