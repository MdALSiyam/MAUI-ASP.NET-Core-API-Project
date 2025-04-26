using Client.DTOs;
using System.Collections.ObjectModel;
using System.Text.Json;

namespace Client;

public partial class StudentListPage : ContentPage
{
    public ObservableCollection<StudentDto> StudentList { get; set; } = new ObservableCollection<StudentDto>();
    private ImageSource pickedImageSource;
    public ImageSource PickedImageSource
    {
        get => pickedImageSource;
        set
        {
            pickedImageSource = value;
            OnPropertyChanged();
        }
    }
    public StudentListPage()
	{
		InitializeComponent();
        BindingContext = this;
        _ = LoadStudentData();
    }
    private async Task LoadStudentData()
    {
        try
        {
            HttpClient _client = new HttpClient();
            _client.BaseAddress = new Uri(DeviceInfo.Platform == DevicePlatform.Android ? "http://10.0.2.2:5087" : "http://localhost:5087");
            HttpResponseMessage response = await _client.GetAsync("/api/students");
            if (response.IsSuccessStatusCode)
            {
                string content = await response.Content.ReadAsStringAsync();
                var students = JsonSerializer.Deserialize<ObservableCollection<StudentDto>>(content, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
                if (students != null)
                {
                    StudentList = students;
                    StudentListView.ItemsSource = StudentList;

                }
                else
                {
                    await DisplayAlert("Error", "Failed to load student List", "Ok");
                }
            }
        }
        catch (Exception)
        {

            await DisplayAlert("Error", "Error occured to load student List", "Ok");
        }
    }

    private void OnAddStudentClicked(object sender, EventArgs e)
    {
        Navigation.PushAsync(new StudentCreatePage());
    }

    private void OnStudentUpdateClicked(object sender, EventArgs e)
    {
        if (sender is Button button && button.CommandParameter is StudentDto student)
        {
            Navigation.PushAsync(new StudentUpdatePage(student.Id));
        }

    }

    private async void OnStudentDeleteClicked(object sender, EventArgs e)
    {
        if (sender is Button button && button.CommandParameter is StudentDto student)
        {
            bool result = await DisplayAlert("Confirm", $"Are you sure you want to delete {student.Name}?", "Yes", "No");
            if (result)
            {
                try
                {
                    HttpClient _client = new HttpClient();
                    _client.BaseAddress = new Uri(DeviceInfo.Platform == DevicePlatform.Android ? "http://10.0.2.2:5087" : "http://localhost:5087");
                    HttpResponseMessage response = await _client.GetAsync($"/api/students/{student.Id}");
                    if (response.IsSuccessStatusCode)
                    {
                        StudentList.Remove(student);
                        await DisplayAlert("Success", "Student Deleted Successfully", "Ok");
                    }
                    else
                    {
                        await DisplayAlert("Error", "Failed to delete student", "Ok");
                    }

                }
                catch (Exception)
                {
                    await DisplayAlert("Error", "Error occured to delete student", "Ok");
                }
            }
        }
    }
}