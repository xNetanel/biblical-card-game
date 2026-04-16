# Biblical — A Genesis Card Game

A competitive deck-building card game built in Unity 6, inspired by the Book of Genesis.

## Play in Browser
👉 [Play on itch.io](https://xNetanel.itch.io/biblical)
*(Easy Mode only — Hard Mode requires the desktop build below)*

## Download Desktop Version (includes Hard Mode AI)
👉 [Download on GitHub Releases](https://github.com/xNetanel/biblical-card-game/releases)

## Features
- 29 unique cards with battlecries, deathrattles, and special abilities
- Full deck builder with rarity-based copy limits (Common/Rare/Epic/Legendary)
- Easy Mode: random AI opponent
- Hard Mode: LLM-powered opponent via Groq API (llama-3.1-8b-instant)
- Card animations, procedural SFX, and background music
- Jacob → Israel transformation, silence mechanic, scripture spells

## Hard Mode Setup
Hard Mode uses the Groq API. You need a free API key:

1. Get a free key at https://console.groq.com (no credit card required)
2. In the game folder, copy `config.env.example` → rename it to `config.env`
3. Open `config.env` in Notepad and replace `your_key_here` with your key
4. Launch `Biblical.exe`

## Built With
- Unity 6 (Built-in Render Pipeline)
- C#
- Groq API / llama-3.1-8b-instant
- TextMeshPro

## Credits
Music: 
- "Desert City" Kevin MacLeod (incompetech.com)
Licensed under Creative Commons: By Attribution 4.0 License

- "Ancient Mystery Waltz (Presto)" Kevin MacLeod (incompetech.com)
Licensed under Creative Commons: By Attribution 4.0 License
