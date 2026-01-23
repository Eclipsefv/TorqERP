import { PrismaClient } from '@prisma/client';
const p = new PrismaClient();
console.log("Intentando conectar...");
p.$connect().then(() => console.log("✅ CONECTADO")).catch(e => console.log("❌ ERROR", e));