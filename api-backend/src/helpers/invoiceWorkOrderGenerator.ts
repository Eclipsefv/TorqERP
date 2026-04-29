import { prisma } from '../config/db.js';
/*
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
*/

type PrefixConfig = {
  model: string;
  field: string;
};
 
const PREFIX_MAP: Record<string, PrefixConfig> = {
  W:   { model: 'workOrder',    field: 'orderNumber'       },
  F:   { model: 'invoice',      field: 'invoiceNumber'     },
  ALB: { model: 'deliveryNote', field: 'internalNumber'    },
};

export default async function getNextSequence(
  prisma: any,
  prefix: string
): Promise<string> {
  const config = PREFIX_MAP[prefix];
  if (!config) {
    throw new Error(`Unknown sequence prefix: "${prefix}". Add it to PREFIX_MAP.`);
  }
 
  const year         = new Date().getFullYear();
  const searchPrefix = `${prefix}-${year}-`;
 
  const lastRecord = await prisma[config.model].findFirst({
    where: {
      [config.field]: { startsWith: searchPrefix },
    },
    orderBy: {
      [config.field]: 'desc',
    },
  });
 
  let nextNumber = 1;
 
  if (lastRecord) {
    const lastCode     = lastRecord[config.field] as string;
    const parts        = lastCode.split('-');
    const lastSequence = parseInt(parts[parts.length - 1], 10);
    if (!isNaN(lastSequence)) {
      nextNumber = lastSequence + 1;
    }
  }
 
  const sequenceStr = nextNumber.toString().padStart(4, '0');
  return `${searchPrefix}${sequenceStr}`;
}