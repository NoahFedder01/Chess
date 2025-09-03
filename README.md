# Unity Chess Game

A Unity chess game with draggable pieces that snap to board squares.

## Features

- **Draggable Chess Pieces**: Click and drag any piece to move it
- **Square Snapping**: Pieces automatically snap to the nearest valid square
- **Piece Capturing**: Drag a piece onto an opponent's piece to capture it
- **Visual Feedback**: Pieces scale up slightly when being dragged
- **FEN Support**: Initialize board positions using FEN notation

## Setup Instructions

1. **Scene Setup**:
   - Create an empty GameObject and add the `GameManager` script
   - Create another empty GameObject and add the `CreateChessBoard` script
   - Create a third empty GameObject and add the `ChessBoard` script

2. **Camera Setup**:
   - Position your camera to view the board (suggested position: `(0, 0, -10)`)
   - Set camera projection to Orthographic
   - Adjust the camera size to fit the board (suggested size: 5-8)

3. **UI Setup** (Optional):
   - Create a Canvas and Text element for FEN display
   - Assign the Text component to the `fenText` field in GameManager

4. **Chess Piece Assets**:
   - Place chess piece sprites in the `Assets/Resources/` folder
   - Required naming convention:
     - `white-king.png`, `white-queen.png`, `white-rook.png`, etc.
     - `black-king.png`, `black-queen.png`, `black-rook.png`, etc.

## Scripts Overview

### GameManager
- Manages the overall game state
- Handles FEN string parsing and display
- Initializes the chess board with pieces

### ChessBoard
- Manages the 8x8 board state
- Handles piece movement and capturing
- Converts between world positions and board coordinates
- Provides validation for piece placement

### ChessPiece
- Individual piece behavior
- Handles mouse input for dragging
- Provides visual feedback during movement
- Communicates with ChessBoard for movement validation

### CreateChessBoard
- Creates the visual board squares
- Generates alternating light and dark squares
- Creates individual GameObjects for each square

## How to Use

1. **Start the Game**: The board will automatically initialize with the standard chess starting position
2. **Move Pieces**: Click and drag any piece to move it
3. **Capture Pieces**: Drag a piece onto an opponent's piece to capture it
4. **Invalid Moves**: If you try to make an invalid move, the piece will return to its original position

## Customization

### Board Appearance
- Modify `darkColor` and `lightColor` in CreateChessBoard script
- Adjust `squareSize` to change the size of board squares

### Piece Behavior
- Change `dragScale` in ChessPiece script to adjust how much pieces scale when dragged
- Modify `snapDistance` to change how close pieces need to be to snap to squares

### Game Rules
- Currently allows any piece to move to any valid square
- To implement proper chess rules, modify the `CalculateValidMoves()` method in ChessBoard script

## Current Limitations

- No chess rule validation (pieces can move anywhere that's not occupied by own pieces)
- No check/checkmate detection
- No turn-based gameplay (any piece can be moved at any time)
- No castling, en passant, or pawn promotion

## Future Enhancements

- Implement proper chess movement rules for each piece type
- Add turn-based gameplay
- Implement special moves (castling, en passant, pawn promotion)
- Add check and checkmate detection
- Add move history and undo functionality
- Add AI opponent
- Add multiplayer support
