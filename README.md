# 🚀 URL Shortener with .NET 9, Redis & RabbitMq

یک سرویس کوتاه‌کننده لینک مدرن که با **.NET 9** ساخته شده و شامل قابلیت‌های پیشرفته مثل **Rate Limiting** با استفاده از **Redis + Lua Script** است.

---

## ✨ ویژگی‌ها

- کوتاه‌کردن لینک‌های طولانی به آدرس‌های کوتاه و قابل اشتراک‌گذاری
- API دوستانه و مقیاس‌پذیر برای استفاده در فرانت‌اندها (مثل Next.js)
- **Token Bucket Rate Limiting** برای جلوگیری از abuse
- **سیستم Click Tracking با RabbitMQ**
  - هر کلیک روی لینک‌ها در صف RabbitMQ ذخیره می‌شود
  - یک **Background Service** کلیک‌ها را از صف می‌خواند و به دیتابیس اضافه می‌کند
  - این داده‌ها برای **Analytics** استفاده می‌شوند (مثل تعداد بازدید هر لینک)
- پیاده‌سازی **Atomic Operations** در Redis با Lua Script
- ذخیره لینک‌ها و متادیتا در دیتابیس (default MongoDb)
- **Caching لینک‌های پربازدید**: ذخیره لینک‌ها و شمارش بازدیدها در Redis با استفاده از Sorted Set برای ارائه سریع Top N Links بدون فشار روی دیتابیس.


---

## ⚙️ پیش‌نیازها

- [.NET 9 SDK](https://dotnet.microsoft.com/)
- [Docker (for redis and rabbitmq)](https://www.docker.com/)
- [Redis](https://redis.io/)
- [RabbitMQ](https://www.rabbitmq.com/)
- [MongoDb](https://www.mongodb.com/)
- [Postman](https://www.postman.com/) یا ابزار مشابه برای تست API

---

## 🚀 نصب و اجرا

1. **کلون کردن پروژه**
```bash
git clone https://github.com/MehdiMasoumii/Url-Shortener.git
cd Url-Shortener
```

2. **راه‌اندازی Redis , RabbitMq (با Docker)**
```bash
docker compose -f compose.yaml up -d
```

3. **اجرای پروژه**
```bash
dotnet run --project WebApi
```


## 🛡 Rate Limiting با Redis + Lua

این پروژه از الگوریتم **Token Bucket** استفاده می‌کند  
تا تعداد درخواست‌های هر کاربر در یک بازه زمانی مشخص محدود شود.  

ویژگی کلیدی:
- ذخیره‌سازی آخرین زمان refill در Redis (`last_refill`)
- محاسبه تعداد توکن‌های جدید بر اساس `TimeOffset`
- انجام کل عملیات (چک کردن و آپدیت) به‌صورت **Atomic** با Lua Script

نمونه اسکریپت Lua:
```lua
local data = redis.call('HGETALL', KEYS[1]);
  if #data == 0 then
    redis.call('HSETEX', KEYS[1], 'EX', ARGV[1], 'FIELDS', 2, ARGV[2], ARGV[3], ARGV[4], ARGV[5]);
    return 1;
  else
    local offset = tonumber(ARGV[5]) - tonumber(data[2]);
    local newTokenCount = math.min(math.floor(offset / tonumber(ARGV[6])), tonumber(ARGV[3])) + tonumber(data[4]) - 1;

    if newTokenCount >= 0 then
      redis.call('HSETEX', KEYS[1], 'EX', ARGV[1], 'FIELDS', 2, ARGV[2], math.min(newTokenCount, tonumber(ARGV[3])), ARGV[4], ARGV[5]);
      return 1;
    else
      return 0;
  end
end
```

---

## 🔑 احراز هویت و سطح دسترسی

- **کاربر عادی:** می‌تواند لینک ایجاد، ویرایش یا حذف کند (فقط لینک‌های خودش)
* این نسخه mvp هستش و فعلا فقط کاربر معمولی وجود داره، اما در ادامه premium user ها و ... اضافه خواهند شد!
---

## 📌 برنامه‌های آینده (TODO)

- [ ] داشبورد فرانت‌اند با Next.js
- [ ] کش پیشرفته برای لینک‌ها
- [ ] پشتیبانی از لینک‌های موقت (TTL)

---

## 📜 لایسنس
این پروژه تحت لایسنس MIT منتشر شده است.  
استفاده آزاد، تغییر و توزیع مجاز است.

---
