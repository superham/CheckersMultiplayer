﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CheckersBoardLogic : MonoBehaviour
{

    public Piece[,] pieces = new Piece[8, 8];
    public GameObject whitePiecePrefab;
    public GameObject blackPiecePrefab;
    public bool forceJump;
    public bool isWhite;
    public GameObject whiteTeamScoreTxt;
    public GameObject blackTeamScoreTxt;
    public GameObject whiteNot;
    public GameObject blackNot;

    public int whiteScore;
    public int blackScore;

  
    private Vector3 boardOffset = new Vector3(-4.0f, 0.29f, -4.0f);
    private Vector3 pieceOffset = new Vector3(0.5f, 0, 0.5f);

    private Piece selectedPiece;
    private List<Piece> forcedPieces;

    private Vector2 mouseOver;
    private Vector2 startDrag;
    private Vector2 endDrag;

    private bool isWhiteTurn;
    private bool hasKilled;

    private void updateMouseOver()
    {
        // If its my turn

        if(!Camera.main)
        {
            Debug.Log("Unable to find main camera.");
            return;
        }

        RaycastHit hit;
        if(Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit, 25.0f, LayerMask.GetMask("Board")))
        {
            mouseOver.x = (int)(hit.point.x - boardOffset.x);
            mouseOver.y = (int)(hit.point.z - boardOffset.z);
        }
        else
        {
            mouseOver.x = -1;
            mouseOver.y = -1;
        }
    }
    private void UpdatePieceDrag(Piece p)
    {

        if(!Camera.main)
        {
            Debug.Log("Unable to find main camera.");
            return;
        }

        RaycastHit hit;
        if(Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit, 25.0f, LayerMask.GetMask("Board")))
        {
            p.transform.position = hit.point + Vector3.up;
        }
    }
    private void SelectPiece(int x, int y)
    {
        // Out of bound
        if(x < 0 || x >= 8 || y < 0 || y >= 8)
            return;

        Piece p = pieces[x,y];
        if(p != null && p.isWhite == isWhite)
        {
            if(forcedPieces.Count == 0)
            {
                selectedPiece = p;
                startDrag = mouseOver;
                Debug.Log(selectedPiece.name);
            }
            else
            {
                // Look for the piece under our forced pieces list only if forceJump is true
                if( forceJump?(forcedPieces.Find(fp => fp == p) == null):false)
                    return;

                selectedPiece = p;
                startDrag = mouseOver;
            }
        }
    }
    private void TryMove(int x1, int y1, int x2, int y2)
    {
        forcedPieces = scanForPossibleMove();

        // Multiplayer Support
        startDrag = new Vector2(x1, y1);
        endDrag = new Vector2(x2, y2);
        selectedPiece = pieces[x1,y1];

        //MovePiece(selectedPiece, x2, y2);
        // Out of bounds
        if(x2 < 0 || x2 >= 8 || y2 < 0 || y2 >= 8)
        {
            if(selectedPiece != null)
                MovePiece(selectedPiece, x1, y1); // move piece back to start click pos.

            startDrag = Vector2.zero;
            selectedPiece = null;
            return;
        }

        if (selectedPiece != null)
        {
            // If piece has not moved
            if(endDrag == startDrag){
                MovePiece(selectedPiece, x1, y1);
                startDrag = Vector2.zero;
                selectedPiece = null;
                return;
            }

            // Check if a valid move
            if(selectedPiece.ValidMove(pieces, x1, y1, x2, y2))
            {
                // Did we kill anything
                // If this is a jump
                if(Mathf.Abs(x2-x1) == 2)
                {
                    Piece p = pieces[(x1 + x2)/2, (y1+y2)/2];
                    if(p != null)
                    {
                        pieces[(x1 + x2)/2, (y1+y2)/2] = null;
                        Destroy(p.gameObject);
                        hasKilled = true;
                        // Add one to the score
                        if(isWhiteTurn)
                        {
                            whiteScore++;
                            whiteTeamScoreTxt.GetComponent<Text>().text = whiteScore.ToString();
                            //whiteTeamScoreTxt.GetComponent("Text") = whiteScore.ToString;
                            //whiteTeamScoreTxt.GetComponent() = whiteScore.ToString;
                        }
                        else
                        {
                            blackScore++;
                            blackTeamScoreTxt.GetComponent<Text>().text = blackScore.ToString();
                        }

                    }
                }

                // Were were supposted to kill anything?
                if(forcedPieces.Count != 0 && !hasKilled && forceJump)
                {
                    MovePiece(selectedPiece, x1, y1);
                    startDrag = Vector2.zero;
                    selectedPiece = null;
                    return;
                }

                pieces[x2, y2] = selectedPiece;
                pieces[x1,y1] = null;
                MovePiece(selectedPiece, x2, y2);

                EndTurn();
            }
            else
            { // if not a valid move move its pos back to start of drag
                MovePiece(selectedPiece, x1, y1);
                startDrag = Vector2.zero;
                selectedPiece = null;
                return;
            }
        }
    }
    private void EndTurn()
    {
        int x = (int)endDrag.x;
        int y = (int)endDrag.y;

        // Promotions
        if(selectedPiece != null)
        {
            if(selectedPiece.isWhite && !selectedPiece.isKing && y == 7) // White piece just landed in the promation zone
            {
                selectedPiece.isKing = true;
                selectedPiece.transform.Rotate(Vector3.right * 180);
            }
            else if(!selectedPiece.isWhite && !selectedPiece.isKing && y == 0) // Black piece just landed in the promation zone
            {
                selectedPiece.isKing = true;
                selectedPiece.transform.Rotate(Vector3.right * 180);
            }           
        }

        selectedPiece = null;
        startDrag = Vector2.zero;

        if(scanForPossibleMove(selectedPiece, x, y).Count != 0 && hasKilled)
            return;

        isWhiteTurn = !isWhiteTurn;
        //update not
        if(isWhiteTurn)
        {
            whiteNot.GetComponent<Text>().text = "!";
            blackNot.GetComponent<Text>().text = "";
        }else
        {
            whiteNot.GetComponent<Text>().text = "";
            blackNot.GetComponent<Text>().text = "!";
        }

        isWhite = !isWhite;
        hasKilled = false;
        CheckVictory();
    }
    private void CheckVictory()
    {
        var ps = FindObjectsOfType<Piece>();
        bool hasWhite = false, hasBlack = false;
        for (int i = 0; i < ps.Length; i++)
        {
            if(ps[i].isWhite)
                hasWhite = true;
            else
                hasBlack = true;
        }

        if(!hasWhite)
            Victory(false);
        if(!hasBlack)
            Victory(true);
    }
    private void Victory(bool isWhite)
    {
        if(isWhite)
            Debug.Log("White team has won!");
        else
            Debug.Log("Black team has won!");
    }
    private List<Piece> scanForPossibleMove(Piece p, int x, int y)
    {
        forcedPieces = new List<Piece>();

        if (pieces[x,y].isForcedToMove(pieces, x, y))
            forcedPieces.Add(pieces[x,y]);

        return forcedPieces;
    }
    private List<Piece> scanForPossibleMove()
    {
        forcedPieces  = new List<Piece>(); // we refresh list every scan

        // Check all the pieces
        for(int i=0; i<8; i++)
        {
            for(int j=0; j<8; j++){
                if(pieces[i,j] != null && pieces[i,j].isWhite == isWhiteTurn)
                    if(pieces[i,j].isForcedToMove(pieces, i, j))
                        forcedPieces.Add(pieces[i, j]);
            }
        }
        return forcedPieces;
    }
    private void GenerateBoard() 
    {
        // Generate White team
        for (int y = 0; y < 3; y++)
        {
            bool oddRow = (y % 2 == 0);
            for (int x = 0; x < 8; x += 2)
            {
                // Generate the checker piece
                GeneratePiece((oddRow) ? x : x + 1, y);
            }
        }

        // Generate Black team
        for (int y = 7; y > 4; y--)
        {
            bool oddRow = (y % 2 == 0);
            for (int x = 0; x < 8; x += 2)
            {
                // Generate the checker piece
                GeneratePiece((oddRow) ? x : x + 1, y);
            }
        }
    }
    private void GeneratePiece(int x, int y)
    {
        bool isPieceWhite = (y > 3) ? false : true;
        GameObject go = Instantiate((isPieceWhite) ? whitePiecePrefab : blackPiecePrefab) as GameObject;
        go.transform.SetParent(transform); // makes children of the board
        Piece p = go.GetComponent<Piece>();
        pieces[x,y] = p;
        MovePiece(p, x, y);
    }
    private void MovePiece(Piece p, int x, int y)
    {
        p.transform.position = (Vector3.right * x) + (Vector3.forward * y) + boardOffset + pieceOffset;
    }
    void Start()
    {
        isWhite = true;
        isWhiteTurn = true;
        whiteNot.GetComponent<Text>().text = "!";
        forcedPieces = new List<Piece>();

        whiteScore = 0;
        blackScore = 0;

        GenerateBoard();
    }
    void Update()
    {

        // Debug.Log(Input.mousePosition);
        updateMouseOver();

        //Debug.Log(mouseOver);

        // If it my turn
        if((isWhite) ?isWhiteTurn:!isWhiteTurn)
        {
            int x = (int)mouseOver.x;
            int y = (int)mouseOver.y;

            if(selectedPiece != null)
                UpdatePieceDrag(selectedPiece);

            if (Input.GetMouseButtonDown(0))
            {
                SelectPiece(x, y);
                Debug.Log("Mouse Down.");
            }


            if(Input.GetMouseButtonUp(0))
                TryMove((int)startDrag.x, (int)startDrag.y, x, y);
        }
    }
}
