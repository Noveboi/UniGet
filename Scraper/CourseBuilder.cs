using FileManagers;
using HtmlAgilityPack;
using System.Diagnostics;
using System.Web;
using University;
using Network;

namespace Scraper
{
    public class CourseBuilder
    {
        private const string ATTR_NOT_FOUND = "___attributenotfound___";
        private const string _mainSite = "https://gunet2.cs.unipi.gr";
        private const string _modulesAuth = $"{_mainSite}/modules/auth";
        private static List<(string CourseName, string CourseHref, string CourseID)> coursesInfo = new();

        /// <summary>
        /// Get the courses specified in the list (by name)
        /// </summary>
        /// <returns></returns>
        public async Task<List<Course>> GetCoursesBulkAsync(List<string> courseNames, bool autoWrite = true)
        {
            List<Course> courses = new();
            List<Task<Course>> tasks = new();
            coursesInfo = await GetCoursesInfoAsync();
            foreach(var cI in coursesInfo)
            {
                if (courseNames.Contains(cI.CourseName))
                    tasks.Add(GetCourseAsync(cI.CourseName, autoWrite));
            }

            await Task.WhenAll(tasks);
            tasks.ForEach(task => courses.Add(task.Result));
            return courses;
        }

        /// <summary>
        /// Get the courses specified in the list (by name)
        /// </summary>
        /// <returns></returns>
        public async Task<List<Course>> GetCoursesAsync(List<string> courseNames, bool autoWrite = true)
        {
            Stopwatch s = Stopwatch.StartNew();

            List<Course> courses = new();
            coursesInfo = await GetCoursesInfoAsync();
            foreach (var cI in coursesInfo)
            {
                if (courseNames.Contains(cI.CourseName))
                    courses.Add(await GetCourseAsync(cI.CourseName, autoWrite));
            }

            s.Stop();
            await AppLogger.WriteLineAsync($"Finished getting courses in {(double)s.ElapsedMilliseconds / 1000}s");
            return courses;
        }

        /// <summary>
        /// Get every available subject for the specified course
        /// </summary>
        private async Task<Course> GetCourseAsync(string courseName, bool autoWrite)
        {
            string courseHref = coursesInfo.Find(c => c.CourseName.Equals(courseName)).CourseHref;

            if (courseName == "ΑΝΑΚΟΙΝΩΣΕΙΣ") 
                return new Course("ΑΝΑΚΟΙΝΩΣΕΙΣ", new List<Subject>());

            await AppLogger.WriteLineAsync($"Scraping subjects from course: {courseName}");
            Stopwatch watch = Stopwatch.StartNew();

            List<Subject> courseSubjects = new();

            if (courseHref.Equals(ATTR_NOT_FOUND))
            {
                throw new Exception("Attribute 'href' not found");
            }
            try
            {
                HtmlNode content = await GetPageContent($"{_modulesAuth}/{courseHref}", "content_main");
                HtmlNodeCollection rows = content.SelectNodes("table[@class='sortable']/tr");

                List<Task<Subject>> tasks = new();

                foreach(var row in rows)
                {
                    HtmlNodeCollection td = row.SelectNodes("td");
                    if (td == null) continue; //skip table header

                    HtmlNode a = td[1].SelectSingleNode("a");

                    string subjectLink;
                    string subjectName;
                    string subjectId;

                    if (a == null) //mark as locked (enrollment required)
                    {
                        subjectLink = string.Empty;
                        subjectName = ReplaceSpecialHtmlChars(td[1].GetDirectInnerText());
                    }
                    else
                    {
                        subjectLink = a.GetAttributeValue("href", ATTR_NOT_FOUND);
                        subjectName = ReplaceSpecialHtmlChars(a.InnerText);
                    }
                    subjectId = td[1].SelectSingleNode("small").InnerText.Replace("(",null).Replace(")",null);

                    Subject subject = new()
                    {
                        Name = subjectName,
                        ID = subjectId,
                        Locked = subjectLink.Equals(string.Empty) ? true : false
                    };

                    tasks.Add(GetSubjectContent(subject, subjectLink));
                }

                await Task.WhenAll(tasks);
                tasks.ForEach(task => courseSubjects.Add(task.Result));
                watch.Stop();
                await AppLogger.WriteLineAsync($"Finished getting subjects for {courseName} in {(double)watch.ElapsedMilliseconds / 1000}s");
            }
            catch (HttpRequestException hre)
            {
                await AppLogger.WriteLineAsync($"Couldn't download page content for course {courseName} at: {courseHref}" +
                    $". Exception message: {hre.Message}", AppLogger.MessageType.HandledException);
            }
            catch (IndexOutOfRangeException ioore)
            {
                await AppLogger.WriteLineAsync
                    ($"Index out of range exception for course {courseName}. " +
                    $"Most likely, there is missing data due to bad internet connection. Exception message: {ioore.Message}", 
                    AppLogger.MessageType.HandledException);
            }
            catch (ArgumentException ae)
            {
                await AppLogger.WriteLineAsync
                    ($"Argument exception for course {courseName}." +
                    $"Most likely, there is missing data due to bad internet connection. Exception message: {ae.Message}",
                    AppLogger.MessageType.HandledException);
            }
            catch (Exception ex)
            {
                await AppLogger.WriteLineAsync
                    ($"Other exception type caught for course {courseName}. Exception message: {ex.Message}",
                    AppLogger.MessageType.UnhandledException);
            }

            Course course = new(courseName, courseSubjects);
            if (autoWrite)
                JsonManager.WriteCourseToFile(course);
            return course;
        }

        /// <summary>
        /// Get Documents, Announcements that are associated with the given subject and add them 
        /// to the subject's properties
        /// </summary>
        private async Task<Subject> GetSubjectContent(Subject subject, string link)
        {
            await AppLogger.WriteLineAsync($"\tGetting content for subject: {subject.Name}");
            Stopwatch watch = Stopwatch.StartNew();

            if (subject.Name == string.Empty && subject.ID == string.Empty)
                throw new Exception("Subject is empty");
            if (link == string.Empty)
                return subject;
            try
            {
                HtmlNode content = await GetPageContent($"{_mainSite}/{link}", "content");
                string xpath = "div[@class='leftnav']" +
                    "/div[@class='navmenu']" +
                    "/div[@class='navcontainer']" +
                    "/ul[@class='navlist']" +
                    "/li";

                HtmlNodeCollection navListItems = content.SelectNodes(xpath);

                string documentsLink = string.Empty;
                string announcementsLink = string.Empty;

                foreach (var navListItem in navListItems)
                {
                    if (navListItem.InnerText.Trim() == "Έγγραφα")
                    {
                        documentsLink =
                            navListItem.SelectSingleNode("div/a").GetAttributeValue("href", ATTR_NOT_FOUND);
                    }
                    if (navListItem.InnerText.Trim() == "Ανακοινώσεις")
                    {
                        announcementsLink =
                            navListItem.SelectSingleNode("div/a").GetAttributeValue("href", ATTR_NOT_FOUND);
                    }
                }

                Folder mainSubjectFolder = await GetDocumentsAsync(documentsLink, subject);
                subject.Documents = mainSubjectFolder.Documents;
            }
            catch (HttpRequestException hre)
            {
                await AppLogger.WriteLineAsync($"Couldn't download page content for subject {subject.Name} at: {link}" +
                $". Exception message: {hre.Message}", AppLogger.MessageType.HandledException);
            }
            catch (IndexOutOfRangeException ioore)
            {
                await AppLogger.WriteLineAsync
                    ($"Index out of range exception for subject {subject.Name}. " +
                    $"Most likely, there is missing data due to bad internet connection. Exception message: {ioore.Message}",
                    AppLogger.MessageType.HandledException);
            }
            catch (ArgumentException ae)
            {
                await AppLogger.WriteLineAsync
                    ($"Argument exception for subject {subject.Name}." +
                    $"Most likely, there is missing data due to bad internet connection. Exception message: {ae.Message}",
                    AppLogger.MessageType.HandledException);
            }
            catch (Exception ex)
            {
                await AppLogger.WriteLineAsync
                    ($"Other exception type caught for subject {subject.Name}. Exception message: {ex.Message}",
                    AppLogger.MessageType.UnhandledException);
            }

            watch.Stop();
            await AppLogger.WriteLineAsync($"\tFinished collecting content for " +
                $"{subject.Name} in {(double)watch.ElapsedMilliseconds / 1000}s");

            return subject;
        }

        /// <summary>
        /// Get all the documents associated with a subject 
        /// </summary>
        private async Task<Folder> GetDocumentsAsync(string link, Subject subject, Folder? folder = null)
        {
            Folder currentFolder;

            //Case where GetDocumentsAsync is called from GetSubjectContent
            if (folder is null)
            {
                currentFolder = new Folder
                    (subject.Name);
            }
            else
            {
                currentFolder = folder;
            }
            try
            {
                HtmlNode content = await GetPageContent($"{_mainSite}/{link}", "content_main");
                string xpath = "table[@class='tbl_alt']/tr[@*]";

                HtmlNodeCollection rows = content.SelectNodes(xpath);
                if (rows == null)
                {
                    throw new Exception($"Document rows null at {link}");
                }

                foreach (var row in rows)
                {
                    HtmlNodeCollection columns = row.SelectNodes("td");
                    string docType = columns[0].SelectSingleNode("img")
                        .GetAttributeValue("alt", ATTR_NOT_FOUND).Trim();
                    string docName = ReplaceSpecialHtmlChars
                        (columns[1].SelectSingleNode("a").InnerText.Trim());
                    string docSize = columns[2].InnerText.Trim();
                    string docDate = columns[3].GetAttributeValue("title", ATTR_NOT_FOUND).Trim();
                    string docDownload = docDownload = _mainSite + HttpUtility.UrlDecode(ReplaceSpecialHtmlChars
                            (columns[4].SelectSingleNode("a")
                            .GetAttributeValue("href", ATTR_NOT_FOUND)));

                    if (!docType.Equals(".dir"))
                    {
                        currentFolder.Documents.Files.Add(new Document(
                            docType, docName, docSize, docDate, docDownload));
                    }
                    else
                    {
                        Folder childFolder = new Folder(docName, docDate);
                        string openDir =
                            ReplaceSpecialHtmlChars(columns[1].SelectSingleNode("a").GetAttributeValue("href", ATTR_NOT_FOUND));
                        childFolder = await GetDocumentsAsync(openDir, subject, childFolder);
                        currentFolder.Documents.Folders.Add(childFolder);
                    }
                }
            }
            catch (HttpRequestException hre)
            {
                await AppLogger.WriteLineAsync($"Couldn't download page content for some document at: {link}" +
                $". Exception message: {hre.Message}", AppLogger.MessageType.HandledException);
            }
            catch (IndexOutOfRangeException ioore)
            {
                await AppLogger.WriteLineAsync
                    ($"Index out of range exception for document at {link}. " +
                    $"Most likely, there is missing data due to bad internet connection. Exception message: {ioore.Message}",
                    AppLogger.MessageType.HandledException);
            }
            catch (ArgumentException ae)
            {
                await AppLogger.WriteLineAsync
                    ($"Argument exception for document at {link}." +
                    $"Most likely, there is missing data due to bad internet connection. Exception message: {ae.Message}",
                    AppLogger.MessageType.HandledException);
            }
            catch (Exception ex)
            {
                await AppLogger.WriteLineAsync
                    ($"Other exception type caught for document at {link}. Exception message: {ex.Message}",
                    AppLogger.MessageType.UnhandledException);
            }

            return currentFolder;
        }

        #region Helper Methods

        public async Task<List<(string, string, string)>> GetCoursesInfoAsync()
        {
            HtmlNode content = await GetPageContent($"{_modulesAuth}/listfaculte.php", "content_main");
            string xpath = "table[@class='tbl_alt']/tr";
            HtmlNodeCollection rows = content.SelectNodes(xpath);
            List<(string, string, string)> coursesInfo = new();
            foreach (var row in rows)
            {
                HtmlNode hyperLink = row.SelectSingleNode("td/a");
                HtmlNode small = row.SelectSingleNode("td/small");
                string courseHref = hyperLink.GetAttributeValue("href", ATTR_NOT_FOUND);
                string courseName = ReplaceSpecialHtmlChars(hyperLink.InnerText);
                string courseId = small.InnerText.Trim().Replace("(", null).Replace(")", null).Substring(0, 3);
                coursesInfo.Add((courseName, courseHref, courseId));
            }

            return coursesInfo;
        }

        private string ReplaceSpecialHtmlChars(string text)
        {
            if (text.Contains("&nbsp;"))
            {
                text = text.Replace("&nbsp;", string.Empty);
                ReplaceSpecialHtmlChars(text);
            }
            else if (text.Contains("&#38;"))
            {
                text = text.Replace("&#38;", "&");
                ReplaceSpecialHtmlChars(text);
            }
            else if (text.Contains("&#039;"))
            {
                text = text.Replace("&#039;", "'");
                ReplaceSpecialHtmlChars(text);
            }
            else if (text.Contains("&amp;"))
            {
                text = text.Replace("&amp;", "&");
                ReplaceSpecialHtmlChars(text);
            }
            else if (text.Contains("&quot;"))
            {
                text = text.Replace("&quot;", "'");
                ReplaceSpecialHtmlChars(text);
            }
            return text;
        }

        private async Task<HtmlNode> GetPageContent(string fullUrl, string div)
        {
            Uri uri = new Uri(fullUrl);
            Downloader dl = new Downloader();
            string html = await dl.DownloadStringAsync(uri);
            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(html);

            HtmlNode content = doc.GetElementbyId(div);

            return content;
        }
        #endregion
    }
}
