# RungGPT
An application to generate PLC ladder logic from natural-language prompts using local AI models

## Summary
A Windows desktop application that generates editable ladder logic for Mitsubishi iQ-R PLCs from natural-language prompts, using a local AI model (via Ollama) so nothing leaves the user's machine. The AI outputs a structured JSON representation of the ladder program, which is validated against iQ-R addressing rules, rendered visually in a interactive editor, and exported as Structured Text for import into GX Works3. Users can edit the rungs by hand or conversationally ask the AI to modify existing logic.

## Core Capabilities
- Natural language to ladder generation
- Schema-constrained JSON output
- Automatic validate and repair loop
- Converstational editing
- Visual ladder editor with click to edit contacts, coils, timers and counters
- Export to Structured Text and device-comments CSV
- Fully offline operation via a local model

## Purpose / Benefits
### For PLC integrators
- Cuts initial program drafting time, especially for boilerplate logic
- For junior engineers, the tools produces a first draft to learn from and refine
- Output is a editable ladder-> Human in the loop -> so engineers stay responsible for the final program

### For organization
- On-premise AI means proprietary control logic, 100 percent Data Confidentiality
- Zero per use API cost; runs on a single workstation GPU
- Standardizes company wide conventions by baking them into the system prompt, improving consistency across the team
