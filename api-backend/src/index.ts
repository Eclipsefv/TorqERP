//d imports
import express from 'express';
import cors from 'cors';
import dotenv from 'dotenv';

//Router imports
import authRouter from './routes/authRouter.js';
import productRouter from './routes/productRouter.js';
import userRouter from './routes/userRouter.js';
import customerRouter from './routes/customerRouter.js';

dotenv.config();

const app = express();
const PORT = process.env.PORT || 3000;

app.use(cors());
app.use(express.json()); 

app.use('/api/auth', authRouter);

//products
app.use('/api/products', productRouter);

//users
app.use('/api/users', userRouter);

//customers
app.use('/api/customers', customerRouter)

//health check
app.get('/', (req, res) => {
  res.send('API TorqERP: Online');
});
//server check
app.listen(PORT, () => {
  console.log(`Server running on: ${PORT}`);
});