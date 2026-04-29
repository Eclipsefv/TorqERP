import { Router } from 'express';
import {createSupplier,getSuppliers,getSupplierById,updateSupplier,createDeliveryNote,addLineToDeliveryNote,getDeliveryNotes,getDeliveryNoteById, updateDeliveryNote} from '../controllers/supplierController.js';
 
const router = Router();
 
router.post('/insert',         createSupplier);
router.get('/getSuppliers',    getSuppliers);
router.get('/getSupplier/:id', getSupplierById);
router.put('/update/:id',      updateSupplier);
router.post('/insertDeliveryNote',    createDeliveryNote);
router.post('/addDeliveryNoteLine',   addLineToDeliveryNote);
router.get('/getDeliveryNotes',       getDeliveryNotes);
router.get('/getDeliveryNote/:id',    getDeliveryNoteById);
router.put('/updateDeliveryNote/:id', updateDeliveryNote);
 
export default router;