namespace Exercise.Model
{
    internal class PagePair
    {
        private static readonly string Empty = new string('a', 0);

        internal Page Page1;
        internal Page Page2;
        internal string PaperCode = Empty;
        internal int Finished;

        internal PagePair(Page p)
        {
            Page1 = p;
            p.Another = p;
        }

        internal int SetPageCode(bool first)
        {
            if (first)
            {
                PaperCode = Page1.PaperCode;
                if (PaperCode == null)
                    return Page2 == null ? 0 : 2;
                else
                {
                    if (Page2 == null)
                        return 1;
                    SyncCode();
                    return 3;
                }
            }
            else
            {
                PaperCode = Page2.PaperCode;
                if (PaperCode != null)
                {
                    Page p = Page1;
                    Page1 = Page2;
                    Page2 = p;
                    Page2.Exception = null;
                    SyncCode();
                    return 3;
                }
                return 0;
            }
        }

        internal int SetAnotherPage(Page p)
        {
            Page2 = p;
            Page1.Another = Page2;
            Page2.Another = Page1;
            if (PaperCode == null)
                return 3;
            if (PaperCode == Empty)
                return 0;
            SyncCode();
            return 2;
        }

        private void SyncCode()
        {
            Page2.PaperCode = Page1.PaperCode;
            Page2.PageIndex = Page1.PageIndex + 1;
            Page2.StudentCode = Page1.StudentCode;
        }

        internal bool Finish(bool first)
        {
            Finished |= first ? 1 : 2;
            return Finished == 3;
        }

        internal void DropPage2()
        {
            Page1.Another = null;
            Page2.Another = null;
        }
    }
}