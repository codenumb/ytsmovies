using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading;

namespace ytsmovies
{
    class YtsApi
    {
        public int count {get; set;}
        public void SortByDateAdded()
        {

        }
        public async Task<List<TodoItem>> RefreshDataAsync()
        {
            Uri uri = new Uri(string.Format(Constants.TodoItemsUrl, string.Empty));
            HttpResponseMessage response = await client.GetAsync(uri);
            if (response.IsSuccessStatusCode)
            {
                string content = await response.Content.ReadAsStringAsync();
                Items = JsonConvert.DeserializeObject<List<TodoItem>>(content);
            }
}
    }
    public class YtsJson
    {
        public string status { get; set; }
        public string status_message { get; set; }
        public _YtsData[] data { get; set; }
        public _YtsMeta meta { get; set; }
    }
    public class _YtsData
    {
        public int movie_count { get; set; }
        public int limit { get; set; }
        public _YtsMovie[] movies { get; set; }
        public int page_number { get; set; }

    }
    public class _YtsMovie
    {
        public int id { get; set; }
        public string url { get; set; }
        public string imdb_code { get; set; }
        public string title { get; set; }
        public string title_english { get; set; }
        public string title_long { get; set; }
        public string slug { get; set; }
        public int year { get; set; }
        public int rating { get; set; }
        public int runtime { get; set; }
        public string[] genres { get; set; }
        public string summary { get; set; }
        public string description_full { get; set; }
        public string synopsis { get; set; }
        public string yt_trailer_code { get; set; }
        public string language { get; set; }
        public string mpa_rating { get; set; }
        public string background_image { get; set; }
        public string background_image_original { get; set; }
        public string small_cover_image { get; set; }
        public string medium_cover_image { get; set; }
        public string large_cover_image { get; set; }
        public string state { get; set; }
        public _YtsTorrent torrents { get; set; }
        public string date_uploaded { get; set; }
        public long date_uploaded_unix { get; set; }
    }
    public class _YtsTorrent
    {
        public string url { get; set; }
        public string hash { get; set; }
        public string quality { get; set; }
        public string type { get; set; }
        public int seeds { get; set; }
        public int peers { get; set; }
        public string size { get; set; }
        public long size_bytes { get; set; }
        public string date_uploaded { get; set; }
        public long date_uploaded_unix { get; set; }
    }
    public class _YtsMeta
    {
        public long server_time { get; set; }
        public string server_timezone { get; set; }
        public int api_version { get; set; }
        public string execution_time { get; set; }
    }
}
