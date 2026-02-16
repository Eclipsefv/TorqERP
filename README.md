# TorqERP
TorqERP is my TFG reimagined in .NET MAUI Blazor, it was originally developed in standard .NET 8.0 (what we studied) but after my internship where we used web technologies (mainly JS with LitElements) I decided to give .NET MAUI Blazor a try.

I am using .NET MAUI Blazor for the frontend and Node.js for the backend. I decided this "hybrid" architecture was better suited for my needs instead of sticking to the Microsoft ecosystem because of my already existing experience with Node.js APIs. As for the frontend, I am fairly proficient with C# for the logic, and the MAUI framework allows me to use web components.

It currently has at least core functionalities (Auth and containerization) though the DB Prisma schema only has users at the moment cause I'm rethinking my initial design.

Docker instructions:

Build the container
- docker compose up --build -d
Injecting prisma schema
docker exec torq_api npx prisma generate
docker exec torq_api npx prisma db push
