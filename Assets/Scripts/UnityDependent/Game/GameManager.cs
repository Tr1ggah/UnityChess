﻿using System.Collections.Generic;
using System.Linq;
using UnityChess;
using UnityEngine;

public class GameManager : MonoBehaviourSingleton<GameManager> {
	public Game Game;
	public Queue<Movement> MoveQueue;
	[SerializeField] private GameEvent NewGameStarted;
	[SerializeField] private PromotionUI promotionUI;
	[SerializeField] private UnityChessDebug unityChessDebug;
	public Piece[] CurrentPieces => Game.BoardList.Last.Value.BasePieceList.OfType<Piece>().ToArray();
	public Board CurrentBoard => Game.BoardList.Last.Value;
	public LinkedList<Turn> PreviousMoves => Game.PreviousMoves;
	
	public void Start() {
		MoveQueue = new Queue<Movement>();
#if GAME_TEST
		StartNewGame(Mode.HvH);
#endif
		
#if DEBUG_VIEW
		unityChessDebug.enabled = true;
#endif
	}

	public void StartNewGame(Mode mode) {
		Game = new Game(mode);
		NewGameStarted.Raise();
	}

	public void OnPieceMoved() {
		Movement move = MoveQueue.Dequeue();

		if (move is SpecialMove specialMove) HandleAssociatedPieceBehavior(specialMove);
		
		Game.ExecuteTurn(move);
	}

	private void HandleAssociatedPieceBehavior(SpecialMove specialMove) {
		switch (specialMove) {
			case CastlingMove castlingMove:
				BoardManager.Instance.CastleRook(castlingMove.AssociatedPiece.Position);
				break;
			case EnPassantMove enPassantMove:
				break;
			case PromotionMove promotionMove:
				promotionMove.AssociatedPiece = PromotionUtil.GeneratePromotionPiece(promotionUI.GetUserSelection(), promotionMove.End, Game.CurrentTurnSide);
				BoardManager.Instance.DestroyPieceAtPosition(promotionMove.End);
				BoardManager.Instance.CreateAndPlacePieceGO(promotionMove.AssociatedPiece);
				break;
		}
	}
}