using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using iTextSharp.text.pdf;
using iTextSharp.text.pdf.parser;
using iTextSharp.text;
using System.Windows.Forms;

/*This is where the different formats of pdf documents are handled.
 * It will find each different section: header, event, team results
 * and retrieve the necessary data and compile the events and their 
 * results into a single column text document.
 */
namespace TrackProject_2._0
{
    class Layout_Manager
    {
        public Layout_Manager(string pdf_file_location)
        {
            //this is where methods will ultimately feed information into
            PdfReader reader = create_PdfReader(pdf_file_location);

        }

        /// <summary>
        /// Creates a new PdfReader for the given file.
        /// </summary>
        /// <param name="pdf_file_location">The file which the PdfReader is made for.</param>
        /// <returns>A PdfReader for the file.</returns>
        private PdfReader create_PdfReader(string pdf_file_location)
        {
            return new PdfReader(@"" + pdf_file_location);
        }

        /// <summary>
        /// Gets the number of pages of the current pdf document.
        /// </summary>
        /// <param name="reader">Current PdfReader</param>
        /// <returns>Number of Pages</returns>
        private int get_number_of_pages(PdfReader reader)
        {
            return reader.NumberOfPages;
        }

        /// <summary>
        /// Creates a rectangle with the given coordinates.
        /// </summary>
        /// <param name="llx">Lower left x-coordinate</param>
        /// <param name="lly">Lower Left y-coordinate</param>
        /// <param name="urx">Uper right x-coordinate</param>
        /// <param name="ury">Upper right y-coordinate</param>
        /// <returns></returns>
        private Rectangle create_rectangle(int llx, int lly, int urx, int ury)
        {
            return new Rectangle(llx, lly, urx, ury);
        }

        /// <summary>
        /// Creates a FilteredTextRenderListener for a given rectangle.
        /// </summary>
        /// <param name="rectangle">Rectangle the Listener is made for.</param>
        /// <returns>FilteredTextRenderListener for the given rectangle.</returns>
        private FilteredTextRenderListener create_FilteredTextRenderListener(Rectangle rectangle)
        {
            RenderFilter[] render_filter = new RenderFilter[1];
            render_filter[0] = new RegionTextRenderFilter(rectangle);
            return new FilteredTextRenderListener(new LocationTextExtractionStrategy(), render_filter);
        }

        /// <summary>
        /// Creates a rectangle which is used to iteratively search for the
        /// top and bottom of the running header. Then the running header
        /// is verified.
        /// </summary>
        /// <param name="reader">The pdf document.</param>
        /// <param name="page_number">Page number to use.</param>
        private void get_running_header_in_pdf_document(PdfReader reader, int page_number)
        {
            //getting the height and width of the page
            Rectangle media_box = reader.GetPageSize(page_number);
            int page_height = Convert.ToInt32(media_box.Height);
            int page_width = Convert.ToInt32(media_box.Width);

            int top_of_running_header = 0;
            int bottom_of_running_header = 0;

            // Iterate through 200 pixels down in hope of finding the header area.
            for (int line_number = 1; line_number < 20; line_number++)
            {
                Rectangle rectangle = create_rectangle(0, page_height - (10 * line_number), page_width, page_height - (10 * (line_number - 1)));

                if (does_rectangle_have_text(rectangle, reader, page_number) == true && top_of_running_header == 0)
                {
                    top_of_running_header = page_height - (10 * line_number - 1);
                }
                // This happens after the top_of_running_header has been set. Goes until "Results" is the only text in the line.
                else if (does_rectangle_say_only_results(rectangle, reader, page_number) == true && top_of_running_header != 0)
                {
                    bottom_of_running_header = page_height - (10 * line_number - 1);
                }
            }

            // Verify that the running header contains the correct info. Otherwise, show an error message.
            if (verify_running_header(0, bottom_of_running_header, page_width, top_of_running_header, reader, page_number) == false)
                MessageBox.Show("Invalid Header");
        }

        /// <summary>
        /// Checks if the given rectangle contains text.
        /// </summary>
        /// <param name="rectangle">Rectangle to be checked.</param>
        /// <param name="reader">The pdf document.</param>
        /// <param name="page_number">Page number to be used.</param>
        /// <returns>True if there is no text in the rectangle.</returns>
        private Boolean does_rectangle_have_text(Rectangle rectangle, PdfReader reader, int page_number)
        {
            FilteredTextRenderListener textExtractionStrategy = create_FilteredTextRenderListener(rectangle);

            string rectangle_area = PdfTextExtractor.GetTextFromPage(reader, page_number, textExtractionStrategy);

            if (rectangle_area.Equals(""))
                return true;
            else
                return false;
        }

        /// <summary>
        /// Checks if rectangle contains only "Results"
        /// </summary>
        /// <param name="rectangle">Rectangle to be checked.</param>
        /// <param name="reader">The pdf document.</param>
        /// <param name="page_number">Page number to be used.</param>
        /// <returns>True if the rectangle only contains "Results"</returns>
        private Boolean does_rectangle_say_only_results(Rectangle rectangle, PdfReader reader, int page_number)
        {
            FilteredTextRenderListener textExtractionStrategy = create_FilteredTextRenderListener(rectangle);

            string rectangle_area = PdfTextExtractor.GetTextFromPage(reader, page_number, textExtractionStrategy);

            if (rectangle_area.Equals("Results"))
                return true;
            else
                return false;
        }

        /// <summary>
        /// Verifies that the found running header is identical on another page.
        /// </summary>
        /// <param name="llx">Lower left x-coordinate of running header rectangle.</param>
        /// <param name="lly">Lower left y-coordinate of running header rectangle.</param>
        /// <param name="urx">Upper Right x-coordinate of running header rectangle.</param>
        /// <param name="ury">Upper Right y-coordinate of running header rectangle.</param>
        /// <param name="reader">The pdf document.</param>
        /// <param name="page_number">Page number to be used.</param>
        /// <returns>True if the headers on both pages match.</returns>
        private Boolean verify_running_header(int llx, int lly, int urx, int ury, PdfReader reader, int page_number)
        {
            Rectangle running_header = create_rectangle(llx, lly, urx, ury);
            FilteredTextRenderListener text_extraction_strategy = create_FilteredTextRenderListener(running_header);

            string rectangle_area_on_given_page = PdfTextExtractor.GetTextFromPage(reader, page_number, text_extraction_strategy);

            string rectangle_area_on_next_page = "";
            if (page_number < get_number_of_pages(reader))
                rectangle_area_on_next_page = PdfTextExtractor.GetTextFromPage(reader, page_number + 1, text_extraction_strategy);
            else if (page_number > 1)
                rectangle_area_on_next_page = PdfTextExtractor.GetTextFromPage(reader, page_number - 1, text_extraction_strategy);

            if (rectangle_area_on_given_page.Equals(rectangle_area_on_next_page))
                return true;
            else
                return false;
        }
    }
}
