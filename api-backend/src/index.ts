import express from 'express';
import cors from 'cors';
import dotenv from 'dotenv';
import authRouter from './routes/authRouter.js';

dotenv.config();

const app = express();
const PORT = process.env.PORT || 3000;

app.use(cors());
app.use(express.json()); 

app.use('/api/auth', authRouter);

//health check
app.get('/', (req, res) => {
  res.send('API TorqERP: Online');
});
//server check
app.listen(PORT, () => {
  console.log(`Server running on: ${PORT}`);
});