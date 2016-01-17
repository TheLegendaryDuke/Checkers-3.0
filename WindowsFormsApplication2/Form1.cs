using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WindowsFormsApplication2
{
    public partial class Form1 : Form
    {
        Form2 fform;
        Point origLoc;
        Point startLoc;
        Point endLoc;
        Board b;
        bool single;
        public int dif;
        public Form1(bool s, int diff, Form2 f2)
        {
            InitializeComponent();
            fform = f2;
            dif = diff;
            single = s;
            b = new Board(this, s);
            this.pictureBox1.SendToBack();
        }
        //the method to call when the mouse is pressed down
        public void down(object sender, MouseEventArgs e)
        {
            origLoc = e.Location;
            PictureBox pb = (PictureBox)sender;
            startLoc = pb.Location;
        }
        //the method to call when the mouse moves
        public void move(object sender, MouseEventArgs e)
        {
            if (e.Button == System.Windows.Forms.MouseButtons.Left)
            {
                PictureBox pb = (PictureBox)sender;
                pb.Left += e.X - origLoc.X;
                pb.Top += e.Y - origLoc.Y;
            }
        }
        //the method to end a single-player game
        public void endSingle()
        {
            if(b.checkLose(1))
            MessageBox.Show("Game Over! You Lose!");
            foreach (Control picb in this.Controls) picb.Enabled = false;
            this.Dispose();
            this.Hide();
            fform.Show();
        }
        //the method to call when the mouse is released
        public void up(object sender, MouseEventArgs e)
        {
            PictureBox pb = (PictureBox)sender;
            endLoc = new Point((e.Location.X + pb.Location.X - 32) / 90 * 90 + 32, (e.Location.Y + pb.Location.Y - 12) / 90 * 90 + 12);
            //check if the move is legal and make the move in "valid"
            int result = b.valid(startLoc, endLoc);
            if (result == 1)
            {
                pb.Location = endLoc;
                b.next();
            }
            else if (result == 0)
            {
                pb.Location = startLoc;
            }
            else if (result == 2)
            {
                pb.Location = endLoc;
                b.cont = 1;
            }
            else
            {
                MessageBox.Show(string.Format("Game Over! {0} Won!", b.turn == 1 ? "Yellow" : "Black"));
                this.Dispose();
                this.Hide();
                fform.Show();
                foreach (Control picb in this.Controls) picb.Enabled = false;
            }
        }
        //change to next player's turn
        private void button1_Click(object sender, EventArgs e)
        {
            b.next();
        }
        //the timer used to enable animation for single player mode
        private void timer1_Tick(object sender, EventArgs e)
        {
            if (b.mode == 1)
            {
                b.firstType();
            }
            else
            {
                b.secondType();
            }
        }






    }
    //a helper class to save a pair of x,y coordinates
    struct coor {
        public int x;
        public int y;
        public coor(int a, int b)
        {
            x = a;
            y = b;
        }
    }
    //a helper class to implement the AI and prevent repetitive coding
    class Tree
    {
        public State state;
        public List<Tree> next;
        public int height;
        public int value;
        public Tree(State s)
        {
            state = new State(s);
            next = new List<Tree>();
            height = 1;
            value = -1001;
        }
        //To generate all the possible states in the future given the current state and how far into the future(count)
        public void populate(int count)
        {
            if (count == 0) return;
            List<State> temp = new List<State>();
            if (height % 2 != 0)
            {
                for (int i = 0; i < 8; i++)
                {
                    state.addStep(i, temp, false);
                }
            }
            else
            {
                for (int i = 8; i < 16; i++)
                {
                    state.addStep(i, temp, false);
                }
            }
            if (temp.Count == 0)
            {
                this.value = height % 2 == 0 ? -1000 : 1000;
            }
            foreach (State st in temp)
            {
                Tree fresh = new Tree(st);
                fresh.height = this.height + 1;
                next.Add(fresh);
            }
            foreach (Tree t in next)
            {
                t.populate(count - 1);
            }
        }
        //to find the "score" or "value" of the current state
        public void calc()
        {
            if (this.next.Count != 0)
            {
                //find the value of its sub-states
                foreach (Tree t in next)
                {
                    t.calc();
                }
            }
            else {
                if (value != -1000 && value != 1000) //the value assigned to "win" or "loss"
                {
                    /*
                     * when the current state does not have any sub-state and the current state
                     * is not an endgame, the current state must be the leaf of the tree. it's value
                     * is the same as the value of its own state
                     */
                    state.eval();
                    value = state.value;
                }
                return;
            }
            //when height is even, it is going to be the computer's turn to make a move
            if (height % 2 == 0)
            {
                //the computer will pick the heighest value of the current state's sub-states 
                int max = -2000;
                foreach (Tree t in next)
                {
                    if (t.value > max)
                    {
                        max = t.value;
                    }
                }
                this.value = max;
                return;
            }
            else
            {
                //the computer assumes the player to pick the situation that benefits him the most
                int min = 2000;
                foreach (Tree t in next)
                {
                    if (t.value < min)
                    {
                        min = t.value;
                    }
                }
                this.value = min;
                return;
            }
        }
    }
    //a helper class to better represent the current status of the board
    class State
    {
        //a list of coors that has been eaten to reach this state
        public List<coor> jumped;
        //a 16*3 array of integers where each int[3] represents the x-coor, y-coor, and ifKinged value of a piece
        //if a piece is eaten, all its values are -1
        public int[,] state;
        //the "value" or "score" of the current state
        //the higher the value, the better the situation is to the computer
        public int value;
        public State() {
            state = new int[16, 3];
            jumped = new List<coor>();
        }
        //see if there is a piece at coordinate (x, y)
        //return -1 if there is nothing, 1 if it belongs to the first player and 2 if it belongs to the second
        public int find(int x, int y) {
            if(x > 7 || y > 7 || x < 0 || y < 0) return -2;
            for(int i = 0; i < 16; i++) {
                if(state[i,0] == x && state[i,1] == y) {
                    return i;
                }
            }
            return -1;
        }
        //copy constructor
        public State(State another) {
            state = new int[16,3];
            for(int i = 0; i < 16; i++) {
                state[i,0] = another.state[i, 0];
                state[i,1] = another.state[i, 1];
                state[i,2] = another.state[i, 2];
            }
            //reset the list of pieces jumped
            this.jumped = new List<coor>();                
        }
        //calculate the value of the current state
        public void eval() {
            value = 0;
            //allG is short for all gone
            bool allG = true;
            for (int i = 0; i < 8; i++)
            {
                allG = allG && (state[i, 0] == -1 && state[i, 1] == -1);
            }
            if (allG)
            {
                value = 1000;
                return;
            }
            for(int i = 0; i < 16; i++) {
                if(i < 8) {
                    if(state[i, 1] != -1) {
                        //if the piece is kinged, it is worth 15 pts, else 10
                        if(state[i,2] == 1)  value -= 15;
                        else value -= 10;
                    }
                }
                else {
                    if(state[i, 1] != -1) {
                        if(state[i,2] == 1)  value += 15;
                        else value += 10;
                    }
                }
            }
        }

        //addStep adds all the possible states that can be derived from the current state when moving the piece i
        //into a predefined List dest 
        public void addStep(int i, List<State> dest, bool done) {
            //done is if the piece had made a move in this turn, done will prevent the piece from making a non-eating move
            bool yellow = i < 8;
            if (state[i, 0] == -1) return;                  //invalid piece
            if(state[i, 2] == 0 && yellow) {                //yellow player moving yellow piece
                int locr = find(state[i, 0] + 1, state[i, 1] + 1);    //the two pieces that a non-king piece can move to without eating other pieces
                int locl = find(state[i, 0] - 1, state[i, 1] + 1);
                //if the right spot is empty
                if(locr == -1 && !done) {                  
                    State next = new State(this);
                    next.state[i, 0]++;
                    next.state[i, 1]++;
                    if(next.state[i, 1] == 7) next.state[i, 2] = 1;
                    dest.Add(next);
                }else if(locr > 7) {                       //if the piece next to it is black
                    int endp = find(state[i, 0] + 2, state[i, 1] + 2);
                    if(endp == -1) {                        //if the destination is empty
                        State next = new State(this);
                        next.state[locr, 0] = -1;
                        next.state[locr, 1] = -1;
                        next.state[i, 0] += 2;
                        next.state[i, 1] += 2;
                        next.jumped = new List<coor>();
                        foreach (coor c in this.jumped)
                        {
                            next.jumped.Add(c);
                        }
                        next.jumped.Add(new coor(state[i, 0] + 2, state[i, 1] + 2));
                        if(next.state[i, 1] == 7) next.state[i, 2] = 1;
                        dest.Add(next);
                        next.addStep(i, dest, true);
                    }
                }
                //if the left spot is empty
                if(locl == -1 && !done) {
                    State next = new State(this);
                    next.state[i, 0]--;
                    next.state[i, 1]++;
                    if(next.state[i, 1] == 7) next.state[i, 2] = 1;
                    dest.Add(next);
                    //if the left spot is black
                }else if(locl > 7) {
                    int endp = find(state[i, 0] - 2, state[i, 1] + 2);
                    if(endp == -1) {
                        State next = new State(this);
                        next.state[locl, 0] = -1;
                        next.state[locl, 1] = -1;
                        next.state[i, 0] -= 2;
                        next.state[i, 1] += 2;
                        next.jumped = new List<coor>();
                        foreach (coor c in this.jumped)
                        {
                            next.jumped.Add(c);
                        }
                        next.jumped.Add(new coor(state[i, 0] - 2, state[i, 1] + 2));
                        if(next.state[i, 1] == 7) next.state[i, 2] = 1;
                        dest.Add(next);
                        next.addStep(i, dest, true);
                    }
                }
                //when it's black's turn
            }else if(state[i, 2] == 0 && !yellow) {
                int locr = find(state[i, 0] + 1, state[i, 1] - 1);
                int locl = find(state[i, 0] - 1, state[i, 1] - 1);
                if(locr == -1 && !done) {
                    State next = new State(this);
                    next.state[i, 0]++;
                    next.state[i, 1]--;
                    if (next.state[i, 1] == 0) next.state[i, 2] = 1;
                    dest.Add(next);
                }else if(locr <= 7 && locr != -1) {
                    int endp = find(state[i, 0] + 2, state[i, 1] - 2);
                    if(endp == -1) {
                        State next = new State(this);
                        next.state[locr, 0] = -1;
                        next.state[locr, 1] = -1;
                        next.state[i, 0] += 2;
                        next.state[i, 1] -= 2;
                        next.jumped = new List<coor>();
                        foreach (coor c in this.jumped)
                        {
                            next.jumped.Add(c);
                        }
                        next.jumped.Add(new coor(state[i, 0] + 2, state[i, 1] - 2));
                        if(next.state[i, 1] == 0) next.state[i, 2] = 1;
                        dest.Add(next);
                        next.addStep(i, dest, true);
                    }
                }
                //left empty
                if(locl == -1 && !done) {
                    State next = new State(this);
                    next.state[i, 0]--;
                    next.state[i, 1]--;
                    if(next.state[i, 1] == 0) next.state[i, 2] = 1;
                    dest.Add(next);
                }
                    //left yellow
                else if (locl <= 7 && locl != -1)
                {
                    int endp = find(state[i, 0] - 2, state[i, 1] - 2);
                    if(endp == -1) {
                        State next = new State(this);
                        next.state[locl, 0] = -1;
                        next.state[locl, 1] = -1;
                        next.state[i, 0] -= 2;
                        next.state[i, 1] -= 2;
                        next.jumped = new List<coor>();
                        foreach (coor c in this.jumped)
                        {
                            next.jumped.Add(c);
                        }
                        next.jumped.Add(new coor(state[i, 0] - 2, state[i, 1] - 2));
                        if(next.state[i, 1] == 0) next.state[i, 2] = 1;
                        dest.Add(next);
                        next.addStep(i, dest, true);
                    }
                }
                //if piece is king
            }else if(state[i, 2] == 1) {
                int[] loc = new int[4];
                loc[0] = find(state[i, 0] + 1, state[i, 1] - 1);
                loc[1] = find(state[i, 0] - 1, state[i, 1] - 1);
                loc[3] = find(state[i, 0] - 1, state[i, 1] + 1);
                loc[2] = find(state[i, 0] + 1, state[i, 1] + 1);
                for(int j = 0; j < 4; j++) {
                    int x = j % 2 == 0 ? 1 : -1;
                    int y = j < 2 ? -1 : 1;
                    if(loc[j] == -1 && !done) {
                        State next = new State(this);
                        next.state[i, 0] += x;
                        next.state[i, 1] += y;
                        dest.Add(next);
                    }else if((loc[j] > 7 && yellow) || (loc[j] <= 7 && !yellow && loc[j] != -1)) {
                        int endp = find(state[i, 0] + 2 * x, state[i, 1] + 2 * y);
                        if(endp == -1) {
                            State next = new State(this);
                            next.state[loc[j], 0] = -1;
                            next.state[loc[j], 1] = -1;
                            next.state[i, 0] += 2 * x;
                            next.state[i, 1] += 2 * y;
                            next.jumped = new List<coor>();
                            foreach (coor c in this.jumped)
                            {
                                next.jumped.Add(c);
                            }
                            next.jumped.Add(new coor(state[i, 0] + 2 * x, state[i, 1] + 2 * y));
                            dest.Add(next);
                            next.addStep(i, dest, true);
                        }
                    }
                }
            }
        }
        //getNextStep determines the step the computer will make given the current status
        //of the board and the difficulty level
        public static State getNextStep(State s, int diff)
        {
            List<State> possible = new List<State>();
            List<Tree> tree = new List<Tree>();
            for (int i = 8; i < 16; i++)
            {
                s.addStep(i, possible, false);
            }
            foreach (State st in possible)
            {
                Tree fresh = new Tree(st);
                tree.Add(fresh);
            }
            //for different difficulty level(lvl3 is disabled due to memory constraint)
            switch (diff)
            {
                case 1:
                    foreach (Tree t in tree)
                    {
                        t.populate(2);
                    }
                    break;
                case 2:
                    foreach (Tree t in tree)
                    {
                        t.populate(4);
                    }
                    break;
                case 3:
                    foreach (Tree t in tree)
                    {
                        t.populate(6);
                    }
                    break;
                default:
                    break;
            }
            foreach (Tree t in tree)
            {
                t.calc();
            }
            int max = -2000;
            int refer = 0;
            int num = 0;
            int c = 0;
            foreach (Tree t in tree)
            {
                if (t.value > max)
                {
                    max = t.value;
                    refer = num;
                }
                if (t.value == max)
                {
                    c++;
                }
                num++;
            }
            if (c != 1)
            {
                num = 0;
                foreach (Tree t in tree)
                {
                    if(t.value == max)
                    {
                        t.state.eval();
                        if (t.state.value > max)
                        {
                            max = t.state.value;
                            refer = num;
                        }
                    }
                    num++;
                }
            }
            if (possible.Count == 0) return null;
            //find the most yielding step and return its state
            return possible[refer];
        }
    }
    //trivial
    class Piece
    {
        /*
         * would've set the fields to private and made the "Board" class friend but c# doesn't have friends
         */
        public int x;
        public int y;
        public int owner;
        public bool king;
        public PictureBox p;
        public void kinged()
        {
            king = true;
            if (owner == 1)
            {
                p.Image = Image.FromFile("./light-king.png");
            }
            else
            {
                p.Image = Image.FromFile("./dark-king.png");
            }
        }
        public Piece(int a, int b, PictureBox pb, int o)
        {
            p = pb;
            x = a;
            y = b;
            owner = o;
            king = false;
        }
    }
    class Board
    {
        public int time;   //time elapsed since piece starts moving (used to enable animation in single player)
        public int mode;   //1 is simple one-step move, 2 is eating
        public int newX;
        public int newY;
        public int turn;
        public int cont;
        private int where;
        private List<coor> inter;
        public int toMove;
        public Piece[] pcs;
        Form1 form;
        bool single;
        Form3 f;
        #region helpers to enable animation
        public void firstType()
        {
            if (time == 1000)
            {
                form.timer1.Stop();
                time = 0;
                pcs[toMove].x = newX;
                pcs[toMove].y = newY;
                return;
            }
            pcs[toMove].p.Location = new Point((newX - pcs[toMove].x) * 9 + pcs[toMove].p.Location.X, (newY - pcs[toMove].y) * 9 + pcs[toMove].p.Location.Y);
            time += 100;
        }
        public void secondType()
        {
            if (time == 1000)
            {
                if (where == inter.Count - 1)
                {
                    form.timer1.Stop();
                    pcs[toMove].x = newX;
                    pcs[toMove].y = newY;
                    return;
                }
                else
                {
                    where++;
                    time = 0;
                    pcs[toMove].x = newX;
                    pcs[toMove].y = newY;
                    newX = inter[where].x;
                    newY = inter[where].y;
                }

            }
            pcs[toMove].p.Location = new Point((newX - pcs[toMove].x) * 9 + pcs[toMove].p.Location.X, (newY - pcs[toMove].y) * 9 + pcs[toMove].p.Location.Y);
            time += 100;
        }
        #endregion
        public void move(State s)
        {
            for (int i = 8; i < 16; i++)
            {
                if (pcs[i].owner == 2 && (pcs[i].x != s.state[i, 0] || pcs[i].y != s.state[i, 1]))
                {
                    toMove = i;
                }
            }
            newX = s.state[toMove, 0];
            newY = s.state[toMove, 1];
            if (s.jumped.Count == 0)
            {
                form.timer1.Interval = 100;
                mode = 1;
                time = 0;
                form.timer1.Start();
            }
            else
            {
                form.timer1.Interval = 100;
                mode = 2;
                time = 0;
                where = 0;
                inter = s.jumped;
                newX = inter[0].x;
                newY = inter[0].y;
                form.timer1.Start();
            }
            for (int i = 0; i < 16; i++)
                {
                    if (pcs[i].owner != 0 && s.state[i, 0] == -1)
                    {
                        pcs[i].owner = 0;
                        pcs[i].p.Enabled = false;
                        pcs[i].p.Hide();
                    }
                    if (!pcs[i].king && s.state[i, 2] == 1) pcs[i].kinged();
                }
        }
        //to end the current turn
        public void next()
        {
            if (single)
            {
                f.Show();
                f.Refresh();
                State news = new State();
                for (int i = 0; i < 16; i++)
                {
                    int owner = pcs[i].owner;
                    if (owner == 0)
                    {
                        news.state[i, 0] = -1;
                        news.state[i, 1] = -1;
                        news.state[i, 2] = -1;
                    }
                    else
                    {
                        news.state[i, 0] = pcs[i].x;
                        news.state[i, 1] = pcs[i].y;
                        news.state[i, 2] = pcs[i].king ? 1 : 0;
                    }
                }
                State final = State.getNextStep(news, form.dif);
                if (final == null) form.endSingle();
                move(final);
                f.Hide();
                cont = 0;
                if (checkLose(1))
                {
                    form.endSingle();
                }
            }
            else
            {
                if (turn == 1)
                {
                     turn = 2;
                    form.label1.Text = "Black's Turn";
                }
                else
                {
                    form.label1.Text = "Yellow's Turn";
                    turn = 1;
                }
                cont = 0;
            }
        }
#region PVP
        public bool checkLose(int player)
        {
            bool lose = true;
            foreach (Piece p in pcs)
            {
                if (p.owner == player) lose = false;
            }
            State news = new State();
            for (int i = 0; i < 16; i++)
            {
                int owner = pcs[i].owner;
                if (owner == 0)
                {
                    news.state[i, 0] = -1;
                    news.state[i, 1] = -1;
                    news.state[i, 2] = -1;
                }
                else
                {
                    news.state[i, 0] = pcs[i].x;
                    news.state[i, 1] = pcs[i].y;
                    news.state[i, 2] = pcs[i].king ? 1 : 0;
                }
            }
            List<State> check = new List<State>();
            for (int i = (player - 1) * 8; i < (player - 1) * 8 + 8; i++)
            {
                news.addStep(i, check, false);
            }
            if (check.Count == 0)
            {
                lose = true;
            }
            return lose;
        }
        public int lookup(int x, int y)
        {
            for (int i = 0; i < 16; i++)
            {
                if (pcs[i].x == x && pcs[i].y == y && pcs[i].owner != 0) return i;
            }
            return -1;
        }
        private int abs(int x)
        {
            return (x >= 0 ? x : -x);
        }
        /* 
         * 0 is fail
         * 1 is success without eating
         * 2 is success with eating not winning
         * 3 is won
         */
        public int valid(Point start, Point end)
        {
            Point a = convert(start);
            Point b = convert(end);
            if (b.X > 7 || b.Y > 7) return 0;
            if ((b.X + b.Y) % 2 == 0) return 0;
            if (lookup(b.X, b.Y) != -1) return 0;
            Piece mover = pcs[lookup(a.X, a.Y)];
            if (mover.owner != turn) return 0;
            if (mover.king)
            {
                int difX = b.X - mover.x;
                int difY = b.Y - mover.y;
                if (abs(difX) == 1 && abs(difY) == 1 && cont == 0)
                {
                    Piece st = pcs[lookup(a.X, a.Y)];
                    st.x += difX;
                    st.y += difY;
                    return 1;
                }
                else if (abs(difY) == 2 && abs(difX) == 2)
                {
                    Point mid = new Point(mover.x + difX / 2, mover.y + difY / 2);
                    if (lookup(mid.X, mid.Y) == -1) return 0;
                    if (pcs[lookup(mid.X, mid.Y)].owner == turn) return 0;
                    else
                    {
                        int waitor = pcs[lookup(mid.X, mid.Y)].owner;
                        pcs[lookup(mid.X, mid.Y)].p.Enabled = false;
                        pcs[lookup(mid.X, mid.Y)].p.Visible = false;
                        pcs[lookup(mid.X, mid.Y)].owner = 0;
                        Piece st = pcs[lookup(a.X, a.Y)];
                        st.x += difX;
                        st.y += difY;
                        if (checkLose(waitor))
                        {
                            return 3;
                        }
                        return 2;
                    }
                }
                return 0;
            }
            else
            {
                int owner = mover.owner;
                int difX = b.X - mover.x;
                int difY = b.Y - mover.y;
                if (owner == 2)
                {
                    if (difY < 0)
                    {
                        if (difY == -1 && abs(difX) == 1 && cont == 0)
                        {
                            Piece st = pcs[lookup(a.X, a.Y)];
                            st.x += difX;
                            st.y += difY;
                            if (b.Y == 0) st.kinged();
                            return 1;
                        }
                        else if (difY == -2 && abs(difX) == 2)
                        {
                            Point mid = new Point(mover.x + difX / 2, mover.y + difY / 2);
                            if (lookup(mid.X, mid.Y) == -1) return 0;
                            if (pcs[lookup(mid.X, mid.Y)].owner == turn) return 0;
                            else
                            {
                                if (lookup(mid.X, mid.Y) == -1) return 0;
                                int waitor = pcs[lookup(mid.X, mid.Y)].owner;
                                pcs[lookup(mid.X, mid.Y)].p.Enabled = false;
                                pcs[lookup(mid.X, mid.Y)].p.Visible = false;
                                pcs[lookup(mid.X, mid.Y)].owner = 0;
                                Piece st = pcs[lookup(a.X, a.Y)];
                                st.x += difX;
                                st.y += difY;
                                if (checkLose(waitor))
                                {
                                    return 3;
                                }
                                if (b.Y == 0) st.kinged();
                                return 2;
                            }
                        }
                        return 0;
                    }
                    return 0;
                }
                else
                {
                    if (difY > 0)
                    {
                        if (difY == 1 && abs(difX) == 1 && cont == 0)
                        {
                            Piece st = pcs[lookup(a.X, a.Y)];
                            st.x += difX;
                            st.y += difY;
                            if (b.Y == 7) st.kinged();
                            return 1;
                        }
                        else if (difY == 2 && abs(difX) == 2)
                        {
                            Point mid = new Point(mover.x + difX / 2, mover.y + difY / 2);
                            if (lookup(mid.X, mid.Y) == -1) return 0;
                            if (pcs[lookup(mid.X, mid.Y)].owner == turn) return 0;
                            else
                            {
                                if (lookup(mid.X, mid.Y) == -1) return 0;
                                int waitor = pcs[lookup(mid.X, mid.Y)].owner;
                                pcs[lookup(mid.X, mid.Y)].p.Enabled = false;
                                pcs[lookup(mid.X, mid.Y)].p.Visible = false;
                                pcs[lookup(mid.X, mid.Y)].owner = 0;
                                Piece st = pcs[lookup(a.X, a.Y)];
                                st.x += difX;
                                st.y += difY;
                                if (checkLose(waitor))
                                {
                                    return 3;
                                }
                                if (b.Y == 7) st.kinged();
                                return 2;
                            }
                        }
                        return 0;
                    }
                    return 0;
                }
            }

        }

        public Point convert(Point inp)
        {
            int x = (inp.X - 32) / 90;
            int y = (inp.Y - 12) / 90;
            Point newp = new Point(x, y);
            return newp;
        }
#endregion
        //the Board constructor
        public Board(Form1 f, bool s)
        {
            this.f = new Form3();
            single = s;
            turn = 1;
            form = f;
            pcs = new Piece[16];
            for (int i = 0; i < 8; i++)
            {
                int x = (i < 4) ? (i % 4 * 2 + 1) : (i % 4 * 2);
                int y = (i < 4) ? 0 : 1;
                PictureBox pb = new PictureBox();
                if (i % 2 == 0)
                {
                    pb.Location = new Point(32 + x * 90, 12 + y * 90);
                }
                else
                {
                    pb.Location = new Point(32 + 90 * x, 12 + y * 90);
                }
                pb.Image = Image.FromFile("./light.png");
                pb.Size = new Size(90, 90);
                GraphicsPath gp = new GraphicsPath();
                gp.AddEllipse(pb.DisplayRectangle);
                pb.Region = new Region(gp);
                form.Controls.Add(pb);
                pb.MouseDown += new MouseEventHandler(form.down);
                pb.MouseMove += new MouseEventHandler(form.move);
                pb.MouseUp += new MouseEventHandler(form.up);
                pcs[i] = new Piece(x, y, pb, 1);
            }
            for (int i = 0; i < 8; i++)
            {
                int x = (i < 4) ? (i % 4 * 2 + 1) : (i % 4 * 2);
                int y = (i < 4) ? 6 : 7;
                PictureBox pb = new PictureBox();
                if (i % 2 == 0)
                {
                    pb.Location = new Point(32 + x * 90, 12 + y * 90);
                }
                else
                {
                    pb.Location = new Point(32 + 90 * x, 12 + y * 90);
                }
                pb.Image = Image.FromFile("./dark.png");
                pb.Size = new Size(90, 90);
                GraphicsPath gp = new GraphicsPath();
                gp.AddEllipse(pb.DisplayRectangle);
                pb.Region = new Region(gp);
                form.Controls.Add(pb);
                if (!single)
                {
                    pb.MouseDown += new MouseEventHandler(form.down);
                    pb.MouseMove += new MouseEventHandler(form.move);
                    pb.MouseUp += new MouseEventHandler(form.up);
                }
                pcs[i + 8] = new Piece(x, y, pb, 2);
            }
        }
    }
}
