## Architecture

### 1. Local runtime
- Ollama: C# app to just make REST calls.

### 2. Local Model
- For now, I want to run it lightweight.  Hence, the lightweight gemma4:e4b.
- Gemma 4 is a brand new model. It has native function-calling support and native system prompt support,
  which makes structured JSON output reliable.


### 3. Folder Structure
--> Models / (LadderProgram, Rung, Element, Device)
--> Services / (OllamaService, LadderValidator, StExporter)
--> ViewModels / (MainViewModel, LadderEditorViewModel)
--> Views / (MainWindow.xaml, LadderCanvas.xaml)
--> Prompts / (system.md, examples.json)
--> Tests / (unit tests for validator + exporter)


### 4. Gemma 4 model
- Turning thinking off for generation, on for refinement: thinking off is faster and the JSON format
  constraints aleady forces structure, For "refine this ladder to add X" calls, thinking on gives better
  results because it's a reasoning-heavy edit.
- Using its native tool-calling for validation feedback: Instead of the text-based repair loop, we can
  expose `validate_ladder(json)` as a tool and let the model call it itself.
