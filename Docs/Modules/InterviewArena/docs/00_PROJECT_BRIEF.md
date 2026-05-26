# 00. Project Brief

## Working Title

**Interview Arena**

Alternative internal names:

- Vector Arena
- WebGL Arena Run
- Unity Interview Lab

## Purpose

This project is a Unity learning and interview preparation module.

It should demonstrate practical knowledge of Unity, C#, WebGL, multiplayer, physics, vector math, architecture, and optimization.

The project should remain small enough to finish, but deep enough to discuss confidently in an interview.

## One-Sentence Pitch

A WebGL multiplayer arena mini-game where players enter through a lightweight account system, prepare in a lobby, choose abilities, and complete a physics-based run controlled by a personal server.

## Main Goals

1. Build a finished WebGL prototype.
2. Use a personal server for login, lobby, and match state.
3. Implement a lobby with ready state and ability selection.
4. Implement player movement with physics and vector math.
5. Implement server-validated match flow.
6. Create clean project architecture suitable for GitHub.
7. Prepare talking points for Unity interviews.

## Non-Goals

Avoid these at the start:

- large open world
- complex progression system
- advanced cosmetics shop
- ranked matchmaking
- full production authentication
- complex anti-cheat
- huge content pipeline
- many maps

The first version should prove the architecture and gameplay loop.

## MVP Scope

### MVP 1: Local Gameplay

- Boot scene
- Player movement
- Camera
- Jump
- Dash
- Ground check
- Physics obstacles
- Simple objective
- Finish portal

### MVP 2: Lobby and Account Prototype

- Login by unique username
- Session token from server
- Lobby creation
- Lobby join/leave
- Ready state
- Ability selection
- Host starts run

### MVP 3: Multiplayer Run

- Players spawn from lobby data
- Basic position synchronization
- Server-owned match state
- Countdown
- Score/result screen
- Reconnect handling stub

### MVP 4: Interview Polish

- README
- Architecture docs
- Project diagrams
- asmdef split
- ScriptableObject configs
- Object pooling
- Physics.NonAlloc usage
- Profiler notes
- WebGL build notes

## Success Criteria

The project is successful when it can be opened from GitHub and the reviewer can understand:

- what the project does
- why it exists
- how it is structured
- what Unity/C# topics it demonstrates
- how client-server flow works
- what trade-offs were made
- what would be improved next
