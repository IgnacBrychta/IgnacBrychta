#define OmezeniJenNaKralePriSachu

using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace FormsChess
{
    public partial class Form1 : Form
    {
        public const int ROWS = 8;
        public const int COLUMNS = 8;
        public ChessPiece[,] chessBoard = new ChessPiece[ROWS, COLUMNS];
        private Button[,] buttonGrid = new Button[ROWS, COLUMNS];
        PlayerColor hracNaRade = PlayerColor.BLACK;
        GameState gameState = GameState.SELECT_MOVING_PIECE;
        Dictionary<int[], Color> changedColors = new Dictionary<int[], Color>();
        int selectedPieceRow = -1;
        int selectedPieceColumn = -1;
        bool jeKralHraceNaTahuSachovan = false;
        readonly Color highlightColor = Color.Aqua;
        readonly Color sachovaciBarva = Color.Red;
        const int pocetTahuBezVyhozeniDoRemizy = 50;
        readonly string prefixTlacitekMimoSachovnici = "NCB";
        ZachovaniBarvyPole zachovaniBarvyPolePredchoziPoziceKrale =
            new ZachovaniBarvyPole(Color.SpringGreen, -1, -1);
        int pocetTahuOdVyhozeni = 0;
        Rook whiteLeftRook = null;
        Rook whiteRightRook = null;
        King whiteKing = null;
        Rook blackLeftRook = null;
        Rook blackRightRook = null;
        King blackKing = null;
        TimeSpan uplynulyCas = new TimeSpan();
        readonly TimeSpan sekunda = new TimeSpan(0, 0, 1);
        TimeSpan casNaTah = new TimeSpan();
        readonly TimeSpan maximalniDelkaTahu = new TimeSpan(0, 2, 0);
        
        public Form1()
        {
            InitializeComponent();
            ConfigButtons();
            ZmenaHraceNaRade();
            timer1.Interval = 1000;
        }
        public void GetRowAndColumnFromButton(string buttonName, out int buttonNumber, out int row, out int column)
        {
            string str_buttonNumber = buttonName.Substring(6);
            buttonNumber = int.Parse(str_buttonNumber);
            row = buttonNumber / ROWS;
            column = buttonNumber % COLUMNS - 1;
            if (column < 0)
            {
                column = 7;
                row--;
            }
        }
        private void ConfigButtons()
        {
            
            for (int i = 0; i < this.Controls.Count; i++)
            {
                var controlItem = this.Controls[i];
                if (controlItem is Button)
                {
                    if (controlItem.Name.StartsWith(prefixTlacitekMimoSachovnici))
                    {
                        continue;
                    }
                    GetRowAndColumnFromButton(controlItem.Name, out int buttonNumber, out int row, out int column);
                    buttonGrid[row, column] = (Button)controlItem;
                    if (buttonNumber > 8 && buttonNumber <= 16)
                    {
                        chessBoard[row, column] = new Pawn(PlayerColor.BLACK, row, column, buttonNumber);
                        controlItem.BackgroundImage = chessBoard[row, column].texture_black;
                    }
                    else if (buttonNumber > 48 && buttonNumber <= 56)
                    {
                        chessBoard[row, column] = new Pawn(PlayerColor.WHITE, row, column, buttonNumber);
                        controlItem.BackgroundImage = chessBoard[row, column].texture_white;
                    } 
                    else if (buttonNumber == 1)
                    {
                        chessBoard[row, column] = new Rook(PlayerColor.BLACK, row, column, buttonNumber);
                        blackLeftRook = (Rook)chessBoard[row, column];
                        controlItem.BackgroundImage = chessBoard[row, column].texture_black;
                    }
                    else if (buttonNumber == 8)
                    {
                        chessBoard[row, column] = new Rook(PlayerColor.BLACK, row, column, buttonNumber);
                        blackRightRook = (Rook)chessBoard[row, column];
                        controlItem.BackgroundImage = chessBoard[row, column].texture_black;
                    }
                    else if (buttonNumber == 57)
                    {
                        chessBoard[row, column] = new Rook(PlayerColor.WHITE, row, column, buttonNumber);
                        whiteLeftRook = (Rook)chessBoard[row, column];
                        controlItem.BackgroundImage = chessBoard[row, column].texture_white;
                    }
                    else if (buttonNumber == 64)
                    {
                        chessBoard[row, column] = new Rook(PlayerColor.WHITE, row, column, buttonNumber);
                        whiteRightRook = (Rook)chessBoard[row, column];
                        controlItem.BackgroundImage = chessBoard[row, column].texture_white;
                    }
                    else if (buttonNumber == 2 || buttonNumber == 7)
                    {
                        chessBoard[row, column] = new Knight(PlayerColor.BLACK, row, column, buttonNumber);
                        controlItem.BackgroundImage = chessBoard[row, column].texture_black;
                    }
                    else if (buttonNumber == 58 || buttonNumber == 63)
                    {
                        chessBoard[row, column] = new Knight(PlayerColor.WHITE, row, column, buttonNumber);
                        controlItem.BackgroundImage = chessBoard[row, column].texture_white;
                    }
                    else if (buttonNumber == 3 || buttonNumber == 6)
                    {
                        chessBoard[row, column] = new Bishop(PlayerColor.BLACK, row, column, buttonNumber);
                        controlItem.BackgroundImage = chessBoard[row, column].texture_black;
                    }
                    else if (buttonNumber == 59 || buttonNumber == 62)
                    {
                        chessBoard[row, column] = new Bishop(PlayerColor.WHITE, row, column, buttonNumber);
                        controlItem.BackgroundImage = chessBoard[row, column].texture_white;
                    }
                    else if (buttonNumber == 4)
                    {
                        chessBoard[row, column] = new Queen(PlayerColor.BLACK, row, column, buttonNumber);
                        controlItem.BackgroundImage = chessBoard[row, column].texture_black;
                    }
                    else if (buttonNumber == 60)
                    {
                        chessBoard[row, column] = new Queen(PlayerColor.WHITE, row, column, buttonNumber);
                        controlItem.BackgroundImage = chessBoard[row, column].texture_white;
                    }
                    else if (buttonNumber == 5)
                    {
                        chessBoard[row, column] = new King(PlayerColor.BLACK, row, column, buttonNumber);
                        blackKing = (King)chessBoard[row, column];
                        controlItem.BackgroundImage = chessBoard[row, column].texture_black;
                    }
                    else if (buttonNumber == 61)
                    {
                        chessBoard[row, column] = new King(PlayerColor.WHITE, row, column, buttonNumber);
                        whiteKing = (King)(chessBoard[row, column]);
                        controlItem.BackgroundImage = chessBoard[row, column].texture_white;
                    }
                }
            }
        }
        private void HighlightPossibleMoves(int row, int column)
        {
            changedColors.Clear();
            //List<int[]> availableMoves = chessBoard[row, column].GetAvailableMoves(ref chessBoard, false);
            List<int[]> availableMoves = chessBoard[row, column].dostupnePohybyBehemSachu;
            if (availableMoves.Count == 0 || 
                !(hracNaRade == PlayerColor.WHITE ? whiteKing : blackKing).jeSachovan
                )
            {
                availableMoves = chessBoard[row, column].GetAvailableMoves(ref chessBoard, false);
            }
            foreach (var item in availableMoves)
            {
                changedColors.Add(item, buttonGrid[item[0], item[1]].BackColor);

                buttonGrid[item[0], item[1]].BackColor = highlightColor;
            }
        }
        private void ResetHighlighting()
        {
            foreach (var item in changedColors)
            {
                buttonGrid[item.Key[0], item.Key[1]].BackColor = item.Value;
            }
        }
        private PlayerColor ZiskatOpacnouBarvu()
        {
            return hracNaRade == PlayerColor.WHITE ? PlayerColor.BLACK : PlayerColor.WHITE;
        }
        private void ZmenaHraceNaRade()
        {
            if (gameState != GameState.KING_SLAIN)
            {
                List<int[]> vsechnyPohyby = ChessPiece.GetAllPossibleMoves(ref chessBoard, hracNaRade, out King king, true);
                jeKralHraceNaTahuSachovan = King.JeKralSachovan(ref vsechnyPohyby, king.row, king.column);
                king.GetAvailableMoves(ref chessBoard, false);
                hracNaRade = ZiskatOpacnouBarvu();
                this.Text = $"Ignácovy šachy: Na řadě je {hracNaRade}.";
                label_hracNaRade_zobrazeni.Text = hracNaRade.ToString();
                label_pocetTahu_indicator.Text = pocetTahuOdVyhozeni.ToString();
                if (pocetTahuOdVyhozeni >= pocetTahuBezVyhozeniDoRemizy)
                {
                    Remiza($"Během {pocetTahuBezVyhozeniDoRemizy} tahů nedošlo k vyhození.");
                }
                BarvyRosadovychTlacitek();
                casNaTah = new TimeSpan(0, 0, 0);

                if (jeKralHraceNaTahuSachovan)
                {
                    bool mohouFigurkyZachranitKRale = MohouFigurkyZachranitKrale();
                    if (!(king.availableMoves.Count == 0 || mohouFigurkyZachranitKRale))
                    {
                        KralUsmrcen(king.row, king.column);
                    } else
                    {
                        zachovaniBarvyPolePredchoziPoziceKrale.color = buttonGrid[king.row, king.column].BackColor;
                        zachovaniBarvyPolePredchoziPoziceKrale.row = king.row;
                        zachovaniBarvyPolePredchoziPoziceKrale.column = king.column;
                        buttonGrid[king.row, king.column].BackColor = sachovaciBarva;
#if Upozorneni
                        MessageBox.Show($"{hracNaRade} král je šachován", "Upozorníčko");
#endif
                    }
                    
                } else 
                {
                    if (zachovaniBarvyPolePredchoziPoziceKrale.color != Color.SpringGreen && zachovaniBarvyPolePredchoziPoziceKrale.row != -1) // změna
                    {
                        buttonGrid[zachovaniBarvyPolePredchoziPoziceKrale.row, zachovaniBarvyPolePredchoziPoziceKrale.column]
                            .BackColor = zachovaniBarvyPolePredchoziPoziceKrale.color;
                        zachovaniBarvyPolePredchoziPoziceKrale.row = -1; // opravdu to tu má být?
                    }
                    if (((King)chessBoard[king.row, king.column]).zakazanePohyby.Count > 1 &&
                        ((King)chessBoard[king.row, king.column]).availableMoves.Count == 0 &&
                        ((King)chessBoard[king.row, king.column]).moznePohybyVsechFigurek.Count == 0
                        )
                    {
                        Remiza(king.row, king.column);
                    }
                }
            }
        }
        private bool MohouFigurkyZachranitKrale()
        {
            // aktualizace pohybů černých figurek
            _ = ChessPiece.GetAllPossibleMoves(ref chessBoard, hracNaRade, out King _, allTiles: false); 
            bool existujeObetavaFigurka = false;
            for (int i = 0; i < 8; i++)
            {
                for (int j = 0; j < 8; j++)
                {
                    bool obetavaFigurka = MuzeFigurkaZachranitKrale(i, j);
                    if (obetavaFigurka)
                    {
                        existujeObetavaFigurka = true;
                    }
                }
            }
            return existujeObetavaFigurka;
        }
        private void KralUsmrcen(int row, int column)
        {
            MessageBox.Show($"{chessBoard[row, column].playerColor} král JE šachován a nemá se kam jinam hnout.", $"{chessBoard[row, column].playerColor} král mrtev");
            this.Text = $"Ignácovy šachy: {chessBoard[row, column].playerColor} prohrává. | Readonly mode";
            gameState = GameState.KING_SLAIN;
            timer1.Stop();
        }
        private void Prohra_CasVyprsel(string text, string caption)
        {
            MessageBox.Show(text, caption);
            this.Text = $"Ignácovy šachy: {hracNaRade} prohrává. | Readonly mode";
            gameState = GameState.KING_SLAIN;
            timer1.Stop();
        }
        private void Remiza(int row, int column)
        {
            MessageBox.Show($"{chessBoard[row, column].playerColor} král NENÍ šachován a nemá se kam jinam hnout.", "Remíza");
            this.Text = "Ignácovy šachy: Remíza | Readonly mode";
            gameState = GameState.KING_SLAIN;
        }
        private void Remiza(string textOverride)
        {
            MessageBox.Show(textOverride, "Remíza");
            this.Text = "Ignácovy šachy: Remíza. | Readonly mode";
            gameState = GameState.KING_SLAIN;
        }
        private void button1_Click(object sender, EventArgs e)
        {
            Button senderButton = (Button)sender;
            GetRowAndColumnFromButton(senderButton.Name, out _, out int row, out int column);
            //if (hracNaRade == chessBoard[row, column]?.playerColor) // sachovan?

            if (gameState == GameState.SELECT_MOVING_PIECE)
            {
                // Souřadnice původní zvolené figurky (source) jsou neplatné && kliknutí není ve sféře vlivu právě zvolené figurky (target)
                if (selectedPieceRow == -1 && selectedPieceColumn == -1 || !changedColors.Any(x => x.Key[0] == row && x.Key[1] == column))
                {
                    if (chessBoard[row, column] is null)
                    {
                        return;
                    }
                    if (chessBoard[row, column].playerColor != hracNaRade)
                    {
                        return;
                    }
#if OmezeniJenNaKralePriSachu
                    if (jeKralHraceNaTahuSachovan)
                    {
                        if (!(bool)(chessBoard[row, column]?.dostupnePohybyBehemSachu.Count > 0) && chessBoard[row, column].GetType() != typeof(King))
                        {
                            return;
                        }
                    }
#endif
                    ResetHighlighting();
                    HighlightPossibleMoves(row, column);
                    selectedPieceColumn = column; // target = source
                    selectedPieceRow = row;
                    gameState = GameState.SELECT_TARGET_TILE;
                }
            }
            else if (gameState == GameState.SELECT_TARGET_TILE)
            {
                if (selectedPieceRow == row && selectedPieceColumn == column) // cancel selection
                {
                    ResetHighlighting();
                    selectedPieceRow = -1;
                    selectedPieceColumn = -1;
                    gameState = GameState.SELECT_MOVING_PIECE;
                    return;
                }
                bool boardChanged = false;
                foreach (var item in changedColors)
                {
                    if (item.Key[0] == row && item.Key[1] == column) // Valid target
                    {
                        if (!(chessBoard[row, column] is null))
                        {
                            pocetTahuOdVyhozeni = 0;
                        } else
                        {
                            pocetTahuOdVyhozeni++;
                        }
                        //if (jeKralHraceNaTahuSachovan &&
                        //    !(bool)chessBoard[selectedPieceRow, selectedPieceColumn]?.dostupnePohybyBehemSachu
                        //    .Any(x => x[0] == row && x[1] == column)
                        //    )
                        //{
                        //    break;
                        //}
                        chessBoard[row, column] = chessBoard[selectedPieceRow, selectedPieceColumn]; // swap piece reference on grid
                        chessBoard[selectedPieceRow, selectedPieceColumn].row = -1;
                        chessBoard[selectedPieceRow, selectedPieceColumn].column = -1;
                        chessBoard[selectedPieceRow, selectedPieceColumn] = null; // delete piece
                        chessBoard[row, column].MovePiece(row, column, ref chessBoard, ref buttonGrid); // move coords in piece reference

                        buttonGrid[row, column].BackgroundImage = chessBoard[row, column].playerColor == PlayerColor.WHITE ? // change background image
                            chessBoard[row, column].texture_white : chessBoard[row, column].texture_black;
                        buttonGrid[selectedPieceRow, selectedPieceColumn].BackgroundImage = null; // reset background image
                        boardChanged = true;
                        if (!timer1.Enabled) // start clock if not started
                        {
                            timer1.Start();
                        }
                        break;
                    }
                }
#if OmezeniJenNaKralePriSachu
                if (jeKralHraceNaTahuSachovan && !boardChanged) // nepovolit pohnutí s jinou figurkou než s králem, když je král šachován
                {
                    //if ( || chessBoard[row, column].GetType() != typeof(King))
                    //{
                    //    return;
                    //}
                    gameState = GameState.SELECT_MOVING_PIECE;
                    ResetHighlighting();
                    return;
                }
#endif
                ResetHighlighting();
                if (!boardChanged)
                {
                    if (chessBoard[row, column] is null) // Pokud je zvolena figurka a poté je zvoleno prázdné pole, resetovat výběr
                    {

                        selectedPieceRow = -1;
                        selectedPieceColumn = -1;
                        gameState = GameState.SELECT_MOVING_PIECE;
                        return; 
                    }
                    if (chessBoard[row, column].playerColor != hracNaRade)
                    {
                        return;
                    }
                    HighlightPossibleMoves(row, column);
                    selectedPieceRow = row;
                    selectedPieceColumn = column;

                } else
                {
                    selectedPieceRow = -1;
                    selectedPieceColumn = -1;
                    if (gameState != GameState.KING_SLAIN)
                    {
                        gameState = GameState.SELECT_MOVING_PIECE;
                    }
                    
                    ZmenaHraceNaRade();
                }
    
            }
        }
        private bool MuzeFigurkaZachranitKrale(int pieceRow, int pieceColumn)
        {
            if (chessBoard[pieceRow, pieceColumn] is null)
            {
                return false;
            }
            if (chessBoard[pieceRow, pieceColumn].playerColor != hracNaRade)
            {
                return false;
            }
            bool pohybNezachraniKrale = false;
            List<int[]> movesOfPiece = chessBoard[pieceRow, pieceColumn].dostupnePohybyBehemSachu;
            ChessPiece buffer = null;
            for (int i = 0; i < movesOfPiece.Count; i++)
            {
                var item = movesOfPiece[i];
                // swap
                buffer = chessBoard[item[0], item[1]];
                chessBoard[item[0], item[1]] = chessBoard[pieceRow, pieceColumn];
                chessBoard[pieceRow, pieceColumn] = null;

                List<int[]> vsechnyPohyby = ChessPiece.GetAllPossibleMoves(ref chessBoard, ZiskatOpacnouBarvu(), out King king, allTiles: false);
                pohybNezachraniKrale = King.JeKralSachovan(ref vsechnyPohyby, king.row, king.column);

                // swap back
                chessBoard[pieceRow, pieceColumn] = chessBoard[item[0], item[1]];
                chessBoard[item[0], item[1]] = buffer;

                if (pohybNezachraniKrale)
                {
                    movesOfPiece.RemoveAt(i);
                    i--; // při smazání pohybu nutno změnit index
                }
                pohybNezachraniKrale = false;
            }
            return movesOfPiece.Count > 0;
        }
        private void Form1_Load(object sender, EventArgs e)
        {

        }
        private void BarvyRosadovychTlacitek()
        {
            if (JeDostupnaRosada(7, 1, 4, whiteLeftRook, whiteKing) &&
                !King.JeKralSachovan(ref whiteKing.moznePohybyVsechFigurek, whiteKing.row, whiteKing.column - 1) &&
                !King.JeKralSachovan(ref whiteKing.moznePohybyVsechFigurek, whiteKing.row, whiteKing.column - 2)
                )
            {
                NCBbutton_dlouhaBilaRosada.BackColor = Color.Green;
                NCBbutton_dlouhaBilaRosada.Enabled = true;
            } else
            {
                NCBbutton_dlouhaBilaRosada.BackColor = Color.Beige;
                NCBbutton_dlouhaBilaRosada.Enabled = false;
            }

            if (JeDostupnaRosada(7, 5, 7, whiteRightRook, whiteKing) &&
                !King.JeKralSachovan(ref whiteKing.moznePohybyVsechFigurek, whiteKing.row, whiteKing.column + 1) &&
                !King.JeKralSachovan(ref whiteKing.moznePohybyVsechFigurek, whiteKing.row, whiteKing.column + 2)
               )
            {
                NCBbutton_kratkaBilaRosada.BackColor = Color.Green;
                NCBbutton_kratkaBilaRosada.Enabled = true;
            }
            else
            {
                NCBbutton_kratkaBilaRosada.BackColor = Color.Beige;
                NCBbutton_kratkaBilaRosada.Enabled = false;
            }

            if (JeDostupnaRosada(0, 1, 4, blackLeftRook, blackKing) &&
                !King.JeKralSachovan(ref blackKing.moznePohybyVsechFigurek, blackKing.row, blackKing.column - 1) &&
                !King.JeKralSachovan(ref blackKing.moznePohybyVsechFigurek, blackKing.row, blackKing.column - 2)
               )
            {
                NCBbutton_dlouhaCernaRosada.BackColor = Color.Green;
                NCBbutton_dlouhaCernaRosada.Enabled = true;
            }
            else
            {
                NCBbutton_dlouhaCernaRosada.BackColor = Color.Beige;
                NCBbutton_dlouhaCernaRosada.Enabled = false;
            }

            if (JeDostupnaRosada(0, 5, 7, blackRightRook, blackKing) &&
                !King.JeKralSachovan(ref blackKing.moznePohybyVsechFigurek, blackKing.row, blackKing.column + 1) &&
                !King.JeKralSachovan(ref blackKing.moznePohybyVsechFigurek, blackKing.row, blackKing.column + 2)
               )
            {
                NCBbutton_kratkaCernaRosada.BackColor = Color.Green;
                NCBbutton_kratkaCernaRosada.Enabled = true;
            }
            else
            {
                NCBbutton_kratkaCernaRosada.BackColor = Color.Beige;
                NCBbutton_kratkaCernaRosada.Enabled = false;
            }
        }
        private bool JeDostupnaRosada(int row, int columnStart, int columnEnd, Rook rook, King king)
        {
            bool volnyProstorProRosadu = true;
            for (int i = columnStart; i < columnEnd; i++)
            {
                if (!(chessBoard[row, i] is null))
                {
                    volnyProstorProRosadu = false;
                    break;
                }
            }
            if (!(bool)rook?.hasMoved && !(bool)king?.hasMoved && 
                volnyProstorProRosadu && !(bool)king?.jeSachovan &&
                king?.playerColor == hracNaRade)
            {
                return true;
            }
            return false;
        }
        private void ProvestKratkouRosadu(King king, Rook rook)
        {
            king.MovePiece(king.row, 6);
            chessBoard[king.row, 6] = king;
            chessBoard[king.row, 4] = null;
            buttonGrid[king.row, 6].BackgroundImage = king.playerColor == PlayerColor.WHITE ? 
                king.texture_white : king.texture_black;
            buttonGrid[king.row, 4].BackgroundImage = null;

            rook.MovePiece(rook.row, 5);
            chessBoard[rook.row, 5] = rook;
            chessBoard[rook.row, 7] = null;
            buttonGrid[rook.row, 5].BackgroundImage = rook.playerColor == PlayerColor.WHITE ?
                rook.texture_white : rook.texture_black;
            buttonGrid[rook.row, 7].BackgroundImage = null;

            ZmenaHraceNaRade();
        }
        private void ProvestDlouhouRosadu(King king, Rook rook)
        {
            king.MovePiece(king.row, 2);
            chessBoard[king.row, 2] = king;
            chessBoard[king.row, 4] = null;
            buttonGrid[king.row, 2].BackgroundImage = king.playerColor == PlayerColor.WHITE ?
                king.texture_white : king.texture_black;
            buttonGrid[king.row, 4].BackgroundImage = null;

            rook.MovePiece(rook.row, 3);
            chessBoard[rook.row, 3] = rook;
            chessBoard[rook.row, 0] = null;
            buttonGrid[rook.row, 3].BackgroundImage = rook.playerColor == PlayerColor.WHITE ?
                rook.texture_white : rook.texture_black;
            buttonGrid[rook.row, 0].BackgroundImage = null;

            ZmenaHraceNaRade();
        }
        private void NCBbutton_kratkaBilaRosada_Click(object sender, EventArgs e)
        {
            ProvestKratkouRosadu(whiteKing, whiteRightRook);
        }
        private void NCBbutton_dlouhaBilaRosada_Click(object sender, EventArgs e)
        {
            ProvestDlouhouRosadu(whiteKing, whiteLeftRook);
        }
        private void NCBbutton_dlouhaCernaRosada_Click(object sender, EventArgs e)
        {
            ProvestDlouhouRosadu(blackKing, blackLeftRook);
        }
        private void NCBbutton_kratkaCernaRosada_Click(object sender, EventArgs e)
        {
            ProvestKratkouRosadu(blackKing, blackRightRook);
        }
        private void timer1_Tick(object sender, EventArgs e)
        {
            BeginInvoke(new Action(() =>
            {
                uplynulyCas += sekunda;
                casNaTah += sekunda;
                label_casomira.Text = uplynulyCas.ToString();
                label_zbyvajiciCasNaTah.Text = (maximalniDelkaTahu - casNaTah).ToString();
                if (casNaTah > maximalniDelkaTahu)
                {
                    timer1.Stop();
                    Prohra_CasVyprsel($"Čas vypršel. {hracNaRade} prohrává.", "Čas vyptšel.");
                }
            }));
        }
    }
    // predikce figurek které král může vyhodit -> když ji vyhodí, může v příštím tahu vyhodit nějaká další?
    internal struct ZachovaniBarvyPole
    {
        internal Color color;
        internal int row;
        internal int column;
        public ZachovaniBarvyPole(Color color, int row, int column)
        {
            this.color= color;
            this.row= row;
            this.column= column;
        }
    }
    public abstract class ChessPiece
    {
        internal static string directory = Directory.GetCurrentDirectory();
        internal PlayerColor playerColor;
        internal abstract Image texture_black { get; }
        internal abstract Image texture_white { get; }
        internal int row { get; set; }
        internal int column { get; set; }
        internal int buttonNumber { get; set; }
        public virtual void MovePiece(int newRow, int newColumn)
        {
            row = newRow; column = newColumn;
        }
        public virtual void MovePiece(int newRow, int newColumn, ref ChessPiece[,] chessBoard, ref Button[,] buttonGrid)
        {
            row = newRow; column = newColumn;
        }
        public bool IsAtTop() => row == 0;
        public bool IsAtBottom() => row == Form1.ROWS - 1;
        public bool IsAtLeftEdge() => column == 0;
        public bool IsAtRightEdge() => column == Form1.ROWS - 1;
        internal List<int[]> dostupnePohybyBehemSachu = new List<int[]>();
        public abstract List<int[]> GetAvailableMoves(ref ChessPiece[,] chessBoard, bool allTiles);
        internal static List<int[]> GetAllPossibleMoves(ref ChessPiece[,] chessBoard, PlayerColor hracNaRade, out King king, bool allTiles = false)
        {
            List<int[]> allAvailableMovesOfAllPiecesOfCurrentPlayer = new List<int[]>();
            king = null;
            for (int i = 0; i < chessBoard.GetLength(0); i++)
            {
                for (int j = 0; j < chessBoard.GetLength(1); j++)
                {
                    bool isKingTile = chessBoard[i, j]?.GetType() == typeof(King);
                    if (chessBoard[i, j]?.playerColor == hracNaRade && !isKingTile)
                    {
                        List<int[]> availableMovesOfPiece = chessBoard[i, j].GetAvailableMoves(ref chessBoard, allTiles);
                        allAvailableMovesOfAllPiecesOfCurrentPlayer = allAvailableMovesOfAllPiecesOfCurrentPlayer.Concat(availableMovesOfPiece).ToList();
                    }
                    if (isKingTile && chessBoard[i, j]?.playerColor != hracNaRade) // kral opacne barvy
                    {
                        king = (King)chessBoard[i, j];
                    }
                }
            }
            return allAvailableMovesOfAllPiecesOfCurrentPlayer;
        }
        internal static Image ScaleImage(Image image, int maxWidth, int maxHeight, bool invertColors = false)
        {
            var ratioX = (double)maxWidth / image.Width;
            var ratioY = (double)maxHeight / image.Height;
            var ratio = Math.Min(ratioX, ratioY);

            var newWidth = (int)(image.Width * ratio);
            var newHeight = (int)(image.Height * ratio);

            var newImage = new Bitmap(maxWidth, maxHeight);
            using (var graphics = Graphics.FromImage(newImage))
            {
                // Calculate x and y which center the image
                int y = (maxHeight / 2) - newHeight / 2;
                int x = (maxWidth / 2) - newWidth / 2;

                // Draw image on x and y with newWidth and newHeight
                graphics.DrawImage(image, x, y, newWidth, newHeight);
                graphics.Dispose();
            }

            if (invertColors)
            {
                for (int y = 0; y <= newImage.Height - 1; y++)
                {
                    for (int x = 0; x <= newImage.Width - 1; x++)
                    {
                        Color inv = newImage.GetPixel(x, y);
                        if (inv.A == 0)
                        {
                            continue;
                        }
                        //inv = Color.FromArgb(255, (255 - inv.R), (255 - inv.G), (255 - inv.B));
                        inv = Color.FromArgb(255, (232), (232), (232));
                        newImage.SetPixel(x, y, inv);
                    }
                }
            }
            return newImage;
        }
    }
    internal class Pawn : ChessPiece
    {
        internal static string texturePath = string.Join("\\", directory.Substring(0, directory.Length - 9)) + "\\textures\\pawn_chess.png";
        internal override Image texture_black { get { return ChessPiece.ScaleImage(Image.FromFile(texturePath), 70, 70); } }
        internal override Image texture_white { get { return ChessPiece.ScaleImage(Image.FromFile(texturePath), 70, 70, true); } } 
        internal bool hasMoved = false;
        public Pawn(PlayerColor playerColor, int x, int y, int buttonNumber)
        {
            this.playerColor = playerColor;
            this.row = x;
            this.column = y;
            this.buttonNumber = buttonNumber;
        }
        public override List<int[]> GetAvailableMoves(ref ChessPiece[,] chessBoard, bool allTiles = false)
        {
            List<int[]> availableMoves = new List<int[]>();
            if (playerColor == PlayerColor.WHITE)
            {
                if (chessBoard[row - 1, column] is null && !allTiles) // IsAtTop // pěšec nemůže vyhazovat před sebou
                {
                    availableMoves.Add(new int[] { row - 1, column });

                    if (!hasMoved && chessBoard[row - 2, column] is null && !allTiles)
                    {
                        availableMoves.Add(new int[] { row - 2, column });
                    }
                }
                if (!IsAtLeftEdge()) 
                {
                    if (!(chessBoard[row - 1, column - 1] is null) || allTiles) // pěšec může jedině vyhazovat v úlopříčce
                    {
                        if (chessBoard[row - 1, column - 1]?.playerColor != playerColor || allTiles)// || (chessBoard[row - 1, column - 1] is null))
                        {
                            availableMoves.Add(new int[] { row - 1, column - 1 });
                        }
                    }
                }
                if (!IsAtRightEdge())
                {
                    if (!(chessBoard[row - 1, column + 1] is null) || allTiles)
                    {
                        if (chessBoard[row - 1, column + 1]?.playerColor != playerColor || allTiles)// || (chessBoard[row - 1, column - 1] is null))
                        {
                            availableMoves.Add(new int[] { row - 1, column + 1 });
                        }
                    }
                }
            }
            else
            {
                if (chessBoard[row + 1, column] is null && !allTiles) // IsAtBottom
                {
                    availableMoves.Add(new int[] { row + 1, column });

                    if (!hasMoved && chessBoard[row + 2, column] is null && !allTiles)
                    {
                        availableMoves.Add(new int[] { row + 2, column });
                    }
                }
                if (!IsAtRightEdge()) // 
                {
                    if (!(chessBoard[row + 1, column + 1] is null) || allTiles)
                    {
                        if (chessBoard[row + 1, column + 1]?.playerColor != playerColor || allTiles)// || (chessBoard[row - 1, column - 1] is null))
                        {
                            availableMoves.Add(new int[] { row + 1, column + 1 });
                        }
                    }
                }
                if (!IsAtLeftEdge())
                {
                    if (!(chessBoard[row + 1, column - 1] is null) || allTiles)
                    {
                        if (chessBoard[row + 1, column - 1]?.playerColor != playerColor || allTiles)// || (chessBoard[row - 1, column - 1] is null))
                        {
                            availableMoves.Add(new int[] { row + 1, column - 1 });
                        }
                    }
                }
            }
            dostupnePohybyBehemSachu = availableMoves;
            return availableMoves;
        }
        private static NahradyZaPesaka TazatSeNeZmenuPesaka()
        {
            Form form2 = new Form2_choosePawnAlternative();
            DialogResult zvolenaFigurka = form2.ShowDialog();
            switch (zvolenaFigurka)
            {
                case DialogResult.OK:
                    return NahradyZaPesaka.QUEEN;
                case DialogResult.Yes:
                    return NahradyZaPesaka.KNIGHT;
                case DialogResult.No:
                    return NahradyZaPesaka.ROOK;
                case DialogResult.Ignore:
                    return NahradyZaPesaka.BISHOP;
            }
            return NahradyZaPesaka.QUEEN;
        }
        private void NahradaZaPesaka(int newRow, int newColumn, ref ChessPiece[,] chessBoard, ref Button[,] buttonGrid)
        {
            NahradyZaPesaka nahradaZaPesaka = TazatSeNeZmenuPesaka();
            switch (nahradaZaPesaka)
            {
                case NahradyZaPesaka.QUEEN:
                    chessBoard[row, column] = new Queen(playerColor, newRow, newColumn, buttonNumber);
                    break;
                case NahradyZaPesaka.ROOK:
                    chessBoard[row, column] = new Rook(playerColor, newRow, newColumn, buttonNumber);
                    break;
                case NahradyZaPesaka.BISHOP:
                    chessBoard[row, column] = new Bishop(playerColor, newRow, newColumn, buttonNumber);
                    break;
                case NahradyZaPesaka.KNIGHT:
                    chessBoard[row, column] = new Knight(playerColor, newRow, newColumn, buttonNumber);
                    break;
            }

            buttonGrid[row, column].BackgroundImage = chessBoard[row, column].playerColor == PlayerColor.WHITE ?
                chessBoard[row, column].texture_white : chessBoard[row, column].texture_black;
        }
        public override void MovePiece(int newRow, int newColumn, ref ChessPiece[,] chessBoard, ref Button[,] buttonGrid)
        {
            hasMoved = true;
            row = newRow;
            column = newColumn;
            if (newRow == 0 && playerColor == PlayerColor.WHITE)
            {
                NahradaZaPesaka(newRow, newColumn, ref chessBoard, ref buttonGrid);
            }
            else if (newRow == Form1.ROWS - 1 && playerColor == PlayerColor.BLACK)
            {
                NahradaZaPesaka(newRow, newColumn, ref chessBoard, ref buttonGrid);
            }
        }

        public override string ToString()
        {
            return $"{playerColor} Pawn at [{row}; {column}] belongs to {buttonNumber}";
        }
    }
    internal class Rook : ChessPiece
    {
        internal static string texturePath = string.Join("\\", directory.Substring(0, directory.Length - 9)) + "\\textures\\rook_chess.png";
        internal override Image texture_black { get { return ChessPiece.ScaleImage(Image.FromFile(texturePath), 70, 70); } }
        internal override Image texture_white { get { return ChessPiece.ScaleImage(Image.FromFile(texturePath), 70, 70, true); } }
        internal bool hasMoved = false;
        public Rook(PlayerColor playerColor, int x, int y, int buttonNumber)
        {
            this.playerColor = playerColor;
            this.row = x;
            this.column = y;
            this.buttonNumber = buttonNumber;
        }
        // rook bishop queen king need to be ale to šach each other
        public override List<int[]> GetAvailableMoves(ref ChessPiece[,] chessBoard, bool allTiles)
        {
            List<int[]> availableMoves = new List<int[]>();
            // Position to bottom
            bool breakNextTile = false;
            for (int i = row + 1; i < 8; i++)
            {
                if (chessBoard[i, column] is null)
                {
                    availableMoves.Add(new int[] { i, column });
                    if (breakNextTile)
                    {
                        break;
                    }
                }
                else if (!(chessBoard[i, column] is null))
                {
                    if (chessBoard[i, column].playerColor != playerColor)
                    {
                        availableMoves.Add(new int[] { i, column });
                    }
                    if (allTiles && chessBoard[i, column].GetType() == typeof(King))
                    {
                        breakNextTile = true;
                    } 
                    else
                    {
                        break;
                    }
                }
            }
            // Position to top
            breakNextTile = false;
            for (int i = row - 1; i >= 0; i--)
            {
                if (chessBoard[i, column] is null)
                {
                    availableMoves.Add(new int[] { i, column });
                    if (breakNextTile)
                    {
                        break;
                    }
                }
                else if (!(chessBoard[i, column] is null))
                {
                    if (chessBoard[i, column].playerColor != playerColor)
                    {
                        availableMoves.Add(new int[] { i, column });
                    }
                    if (allTiles && chessBoard[i, column].GetType() == typeof(King))
                    {
                        breakNextTile = true;
                    }
                    else
                    {
                        break;
                    }
                }
            }
            // Position to right
            breakNextTile = false;
            for (int i = column + 1; i < 8; i++)
            {
                if (chessBoard[row, i] is null)
                {
                    availableMoves.Add(new int[] { row, i });
                    if (breakNextTile)
                    {
                        break;
                    }
                }
                else if (!(chessBoard[row, i] is null))
                {
                    if (chessBoard[row, i].playerColor != playerColor)
                    {
                        availableMoves.Add(new int[] { row, i });
                    }
                    if (allTiles && chessBoard[row, i].GetType() == typeof(King))
                    {
                        breakNextTile = true;
                    }
                    else
                    {
                        break;
                    }
                }
            }
            // Position to left
            breakNextTile = false;
            for (int i = column - 1; i >= 0; i--)
            {
                if (chessBoard[row, i] is null)
                {
                    availableMoves.Add(new int[] { row, i });
                    if (breakNextTile)
                    {
                        break;
                    }
                }
                else if (!(chessBoard[row, i] is null))
                {
                    if (chessBoard[row, i].playerColor != playerColor)
                    {
                        availableMoves.Add(new int[] { row, i });
                    }
                    if (allTiles && chessBoard[row, i].GetType() == typeof(King))
                    {
                        breakNextTile = true;
                    }
                    else
                    {
                        break;
                    }
                }
            }
            dostupnePohybyBehemSachu = availableMoves;
            return availableMoves;
        }
        public override void MovePiece(int newRow, int newColumn)
        {
            base.MovePiece(newRow, newColumn);
            hasMoved = true;
        }
        public override void MovePiece(int newRow, int newColumn, ref ChessPiece[,] chessBoard, ref Button[,] buttonGrid)
        {
            base.MovePiece(newRow, newColumn, ref chessBoard, ref buttonGrid);
            hasMoved = true;
        }
        public override string ToString()
        {
            return $"{playerColor} Rook at [{row}; {column}] belongs to {buttonNumber}";
        }
    }
    internal class Knight : ChessPiece
    {
        internal static string texturePath = string.Join("\\", directory.Substring(0, directory.Length - 9)) + "\\textures\\knight_chess.png";
        internal override Image texture_black { get { return ChessPiece.ScaleImage(Image.FromFile(texturePath), 70, 70); } }
        internal override Image texture_white { get { return ChessPiece.ScaleImage(Image.FromFile(texturePath), 70, 70, true); } }
        public Knight(PlayerColor playerColor, int x, int y, int buttonNumber)
        {
            this.playerColor = playerColor;
            this.row = x;
            this.column = y;
            this.buttonNumber = buttonNumber;
        }

        public override List<int[]> GetAvailableMoves(ref ChessPiece[,] chessBoard, bool _)
        {
            List<int[]> availableMoves = new List<int[]>();
            int[,] jumps = new int[,]
            {
                { -2, 1},
                { -2, -1},
                { 1, 2},
                { -1, 2},
                { 2, 1},
                { 2, -1},
                { 1, -2},
                { -1, -2},
            };
            for (int i = 0; i < jumps.GetLength(0); i++)
            {
                int jumpRow = row + jumps[i, 0];
                int jumpColumn = column + jumps[i, 1];
                if (jumpRow >= 0 && jumpRow <= 7 && jumpColumn >= 0 && jumpColumn <= 7) // platne souradnice
                {
                    if (chessBoard[jumpRow, jumpColumn] is null)
                    {
                        availableMoves.Add(new int[] { jumpRow, jumpColumn });
                    }
                    else if (chessBoard[jumpRow, jumpColumn].playerColor != playerColor)
                    {
                        availableMoves.Add(new int[] { jumpRow, jumpColumn });
                    }
                }
            }
            dostupnePohybyBehemSachu = availableMoves;
            return availableMoves;
        }


        public override string ToString()
        {
            return $"{playerColor} Knight at [{row}; {column}] belongs to {buttonNumber}";
        }
    }
    internal class Bishop : ChessPiece
    {
        internal static string texturePath = string.Join("\\", directory.Substring(0, directory.Length - 9)) + "\\textures\\bishop_chess.png";
        internal override Image texture_black { get { return ChessPiece.ScaleImage(Image.FromFile(texturePath), 70, 70); } }
        internal override Image texture_white { get { return ChessPiece.ScaleImage(Image.FromFile(texturePath), 70, 70, true); } }
        public Bishop(PlayerColor playerColor, int x, int y, int buttonNumber)
        {
            this.playerColor = playerColor;
            this.row = x;
            this.column = y;
            this.buttonNumber = buttonNumber;
        }

        public override List<int[]> GetAvailableMoves(ref ChessPiece[,] chessBoard, bool allTiles)
        {
            List<int[]> availableMoves = new List<int[]>();
            int[,] jumps = new int[,]
            {
                { 1, 1 },
                { -1, 1 },
                { -1, -1 },
                { 1, -1 }
            };
            // NE NW SE SW
            bool breakNextTile = false;
            for (int i = 0; i < jumps.GetLength(0); i++)
            {
                for (int j = 1; j < 8; j++)
                {
                    int jumpRow = row + jumps[i, 0] * j;
                    int jumpColumn = column + jumps[i, 1] * j;
                    if (jumpRow >= 0 && jumpRow <= 7 && jumpColumn >= 0 && jumpColumn <= 7) // platne souradnice
                    {
                        if (chessBoard[jumpRow, jumpColumn] is null)
                        {
                            availableMoves.Add(new int[] { jumpRow, jumpColumn });
                            if (breakNextTile)
                            {
                                break;
                            }
                        }
                        else 
                        {
                            if (chessBoard[jumpRow, jumpColumn].playerColor != playerColor)
                            {
                                availableMoves.Add(new int[] { jumpRow, jumpColumn });
                            }
                            if (allTiles && chessBoard[jumpRow, jumpColumn].GetType() == typeof(King))
                            {
                                breakNextTile = true;
                            }
                            else
                            {
                                break;
                            }
                        }
                    }
                }
            }
            dostupnePohybyBehemSachu = availableMoves;
            return availableMoves;
        }


        public override string ToString()
        {
            return $"{playerColor} Bishop at [{row}; {column}] belongs to {buttonNumber}";
        }
    }
    internal class Queen : ChessPiece
    {
        internal static string texturePath = string.Join("\\", directory.Substring(0, directory.Length - 9)) + "\\textures\\queen_chess.png";
        internal override Image texture_black { get { return ChessPiece.ScaleImage(Image.FromFile(texturePath), 70, 70); } }
        internal override Image texture_white { get { return ChessPiece.ScaleImage(Image.FromFile(texturePath), 70, 70, true); } }
        public Queen(PlayerColor playerColor, int x, int y, int buttonNumber)
        {
            this.playerColor = playerColor;
            this.row = x;
            this.column = y;
            this.buttonNumber = buttonNumber;
        }

        public override List<int[]> GetAvailableMoves(ref ChessPiece[,] chessBoard, bool allTiles)
        {
            List<int[]> availableMoves = new List<int[]>();
            bool breakNextTile = false;
            for (int i = row + 1; i < 8; i++)
            {
                if (chessBoard[i, column] is null)
                {
                    availableMoves.Add(new int[] { i, column });
                    if (breakNextTile)
                    {
                        break;
                    }
                }
                else if (!(chessBoard[i, column] is null))
                {
                    if (chessBoard[i, column].playerColor != playerColor)
                    {
                        availableMoves.Add(new int[] { i, column });
                    }
                    if (allTiles && chessBoard[i, column].GetType() == typeof(King))
                    {
                        breakNextTile = true;
                    }
                    else
                    {
                        break;
                    }
                }
            }
            // Position to top
            breakNextTile = false;
            for (int i = row - 1; i >= 0; i--)
            {
                if (chessBoard[i, column] is null)
                {
                    availableMoves.Add(new int[] { i, column });
                    if (breakNextTile)
                    {
                        break;
                    }
                }
                else if (!(chessBoard[i, column] is null))
                {
                    if (chessBoard[i, column].playerColor != playerColor)
                    {
                        availableMoves.Add(new int[] { i, column });
                    }
                    if (allTiles && chessBoard[i, column].GetType() == typeof(King))
                    {
                        breakNextTile = true;
                    }
                    else
                    {
                        break;
                    }
                }
            }
            // Position to right
            breakNextTile = false;
            for (int i = column + 1; i < 8; i++)
            {
                if (chessBoard[row, i] is null)
                {
                    availableMoves.Add(new int[] { row, i });
                    if (breakNextTile)
                    {
                        break;
                    }
                }
                else if (!(chessBoard[row, i] is null))
                {
                    if (chessBoard[row, i].playerColor != playerColor)
                    {
                        availableMoves.Add(new int[] { row, i });
                    }
                    if (allTiles && chessBoard[row, i].GetType() == typeof(King))
                    {
                        breakNextTile = true;
                    }
                    else
                    {
                        break;
                    }
                }
            }
            // Position to left
            breakNextTile = false;
            for (int i = column - 1; i >= 0; i--)
            {
                if (chessBoard[row, i] is null)
                {
                    availableMoves.Add(new int[] { row, i });
                    if (breakNextTile)
                    {
                        break;
                    }
                }
                else if (!(chessBoard[row, i] is null))
                {
                    if (chessBoard[row, i].playerColor != playerColor)
                    {
                        availableMoves.Add(new int[] { row, i });
                    }
                    if (allTiles && chessBoard[row, i].GetType() == typeof(King))
                    {
                        breakNextTile = true;
                    }
                    else
                    {
                        break;
                    }
                }
            }

            int[,] jumpsBishop = new int[,]
            {
                { 1, 1 },
                { -1, 1 },
                { -1, -1 },
                { 1, -1 }
            };
            breakNextTile = false;
            for (int i = 0; i < jumpsBishop.GetLength(0); i++)
            {
                for (int j = 1; j < 8; j++)
                {
                    int jumpRow = row + jumpsBishop[i, 0] * j;
                    int jumpColumn = column + jumpsBishop[i, 1] * j;
                    if (jumpRow >= 0 && jumpRow <= 7 && jumpColumn >= 0 && jumpColumn <= 7) // platne souradnice
                    {
                        if (chessBoard[jumpRow, jumpColumn] is null)
                        {
                            availableMoves.Add(new int[] { jumpRow, jumpColumn });
                            if (breakNextTile)
                            {
                                break;
                            }
                        }
                        else
                        {
                            if (chessBoard[jumpRow, jumpColumn].playerColor != playerColor)
                            {
                                availableMoves.Add(new int[] { jumpRow, jumpColumn });
                            }
                            if (allTiles && chessBoard[jumpRow, jumpColumn].GetType() == typeof(King))
                            {
                                breakNextTile = true;
                            }
                            else
                            {
                                break;
                            }
                        }
                    }
                }
            }
            dostupnePohybyBehemSachu = availableMoves;
            return availableMoves;
        }
        public override string ToString()
        {
            return $"{playerColor} Queen at [{row}; {column}] belongs to {buttonNumber}";
        }
    }
    internal class King : ChessPiece
    {
        internal static string texturePath = string.Join("\\", directory.Substring(0, directory.Length - 9)) + "\\textures\\king_chess.png";
        internal override Image texture_black { get { return ChessPiece.ScaleImage(Image.FromFile(texturePath), 70, 70); } }
        internal override Image texture_white { get { return ChessPiece.ScaleImage(Image.FromFile(texturePath), 70, 70, true); } }
        internal bool jeSachovan = false;
        internal List<int[]> moznePohybyVsechFigurek = new List<int[]>(); // maximalni počet polí, kam král nesmí vstoupit
        internal static CoordinateComparer comparer = new CoordinateComparer();
        internal List<int[]> availableMoves = new List<int[]>();
        internal List<int[]> zakazanePohyby = new List<int[]>();
        internal bool hasMoved = false;
        public King(PlayerColor playerColor, int x, int y, int buttonNumber)
        {
            this.playerColor = playerColor;
            this.row = x;
            this.column = y;
            this.buttonNumber = buttonNumber;
        }
        private static List<int[]> ZiskatSpolecnePohyby(ref List<int[]> pohyby1, ref List<int[]> pohyby2)
        {
            List<int[]> spolecnePohyby = pohyby1.Intersect(pohyby2, comparer).ToList();
            return spolecnePohyby;
        }
        private static List<int[]> ZiskatRozdilnePohyby(ref List<int[]> pohyby1, ref List<int[]> pohyby2)
        {
            List<int[]> spolecnePohyby = new List<int[]>();
            foreach (var item in pohyby1)
            {
                if (!pohyby2.Contains(item, comparer))
                {
                    spolecnePohyby.Add(item);
                }
            }
            return spolecnePohyby;
        }
        internal static bool JeKralSachovan(ref List<int[]> pohyby, int rowOfKing, int columnOfKing)
        {
            return pohyby.Any(x => x[0] == rowOfKing && x[1] == columnOfKing);
        }
        public override List<int[]> GetAvailableMoves(ref ChessPiece[,] chessBoard, bool _)
        {
            availableMoves.Clear();
            zakazanePohyby.Clear();
            int[,] jumps = new int[,]
            {
                { 1, 1 },
                { 1, -1 },
                { -1, 1 },
                { -1, -1 },
                { 0, 1},
                { 0, -1 },
                { 1, 0 },
                { -1, 0 }
            };
            
            for (int i = 0; i < jumps.GetLength(0); i++)
            {
                int jumpRow = row + jumps[i, 0];
                int jumpColumn = column + jumps[i, 1];
                if (jumpRow >= 0 && jumpRow <= 7 && jumpColumn >= 0 && jumpColumn <= 7)
                {
                    if (chessBoard[jumpRow, jumpColumn] is null)
                    {
                        availableMoves.Add(new int[] { jumpRow, jumpColumn });
                    }
                    else
                    {
                        if (chessBoard[jumpRow, jumpColumn].playerColor != playerColor)
                        {
                            availableMoves.Add(new int[] { jumpRow, jumpColumn });
                        }
                    }
                }
            }
            PlayerColor opacnaBarva = playerColor == PlayerColor.WHITE ? PlayerColor.BLACK : PlayerColor.WHITE;
            moznePohybyVsechFigurek = GetAllPossibleMoves(ref chessBoard, opacnaBarva, out King _, true);
            jeSachovan = JeKralSachovan(ref moznePohybyVsechFigurek, row, column);
            

            zakazanePohyby = ZiskatSpolecnePohyby(ref availableMoves, ref moznePohybyVsechFigurek);
            availableMoves = ZiskatRozdilnePohyby(ref availableMoves, ref zakazanePohyby);
            dostupnePohybyBehemSachu = availableMoves;
            return availableMoves;
        }
        public override void MovePiece(int newRow, int newColumn)
        {
            base.MovePiece(newRow, newColumn);
            hasMoved = true;    
        }
        public override void MovePiece(int newRow, int newColumn, ref ChessPiece[,] chessBoard, ref Button[,] buttonGrid)
        {
            base.MovePiece(newRow, newColumn, ref chessBoard, ref buttonGrid);
            hasMoved = true;
        }
        public override string ToString()
        {
            return $"{playerColor} King at [{row}; {column}] belongs to {buttonNumber}";
        }
    }
    public class CoordinateComparer : IEqualityComparer<int[]>
    {
        public bool Equals(int[] coords1, int[] coords2)
        {
            return coords1[0] == coords2[0] && coords1[1] == coords2[1];
        }

        public int GetHashCode(int[] obj)
        {
            return -1;
        }
    }
    public enum PlayerColor
    {
        WHITE,
        BLACK
    }
    public enum GameState
    {
        SELECT_MOVING_PIECE,
        SELECT_TARGET_TILE,
        KING_SLAIN
    }
    public enum NahradyZaPesaka
    {
        QUEEN,
        ROOK,
        BISHOP,
        KNIGHT
    }
}
