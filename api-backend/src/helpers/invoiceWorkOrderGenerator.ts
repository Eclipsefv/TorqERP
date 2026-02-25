import { prisma } from '../config/db.js';

export default async function getNextSequence(prisma: any, prefix: 'W' | 'F'): Promise<string> {
  const year = new Date().getFullYear();
  const searchPrefix = `${prefix}-${year}-`;

  const lastRecord = await (prefix === 'W' ? prisma.workOrder : prisma.invoice).findFirst({
    where: {
      [prefix === 'W' ? 'orderNumber' : 'invoiceNumber']: {
        startsWith: searchPrefix,
      },
    },
    orderBy: {
      [prefix === 'W' ? 'orderNumber' : 'invoiceNumber']: 'desc',
    },
  });

  let nextNumber = 1;

  if (lastRecord) {
    const lastCode = prefix === 'W' ? (lastRecord as any).orderNumber : (lastRecord as any).invoiceNumber;
    const lastSequence = parseInt(lastCode.split('-')[2]);
    nextNumber = lastSequence + 1;
  }

  const sequenceStr = nextNumber.toString().padStart(4, '0');
  return `${searchPrefix}${sequenceStr}`;
}