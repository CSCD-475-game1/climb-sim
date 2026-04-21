import express from "express";
import cors from "cors";
import OpenAI from "openai";

const app = express();
app.use(cors());
app.use(express.json());

const openai = new OpenAI({
  apiKey: process.env.OPENAI_API_KEY
});

app.post("/npc-chat", async (req, res) => {
  try {
    const { playerText, npcName } = req.body;

    const response = await openai.responses.create({
      model: "gpt-4o-mini",
      input: [
        {
          role: "system",
          content: `lost hiker. goth annoyed slang and sarcastic pop culture. short replies 5-10 words. messy grammar, inconsistent capitalization and typos. reference log, river, wall, tried to climb.` 
        },
        {
          role: "user",
          content: playerText
        }
      ]
    });

    const reply = response.output_text;

    res.json({ reply });
  } catch (err) {
    console.error(err);
    res.status(500).json({ reply: "Something went wrong." });
  }
});

app.listen(3000, () => {
  console.log("Server running on http://localhost:3000");
});
