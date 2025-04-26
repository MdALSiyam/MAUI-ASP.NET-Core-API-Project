using Client.DTOs;
using System.Collections.ObjectModel;
using System.Text;
using System.Text.Json;

namespace Client;

public partial class StudentUpdatePage : ContentPage
{
    private StudentDto studentDto;
    private ImageSource pickedImageSource;
    public ObservableCollection<AddressDto> AddressList { get; set; } = new ObservableCollection<AddressDto>();
    public ImageSource PickedImageSource
    {
        get => pickedImageSource;
        set
        {
            pickedImageSource = value;
            OnPropertyChanged();
        }
    }
    public StudentUpdatePage(int studentId)
	{
		InitializeComponent();
        BindingContext = this;
        _ = LoadStudentData(studentId);
    }
    private async Task LoadStudentData(int studentId)
    {
        try
        {
            HttpClient client = new HttpClient();
            client.BaseAddress = new Uri(DeviceInfo.Platform == DevicePlatform.Android ? "http://10.0.2.2:5087" : "http://localhost:5087");

            HttpResponseMessage response = await client.GetAsync($"/api/students/{studentId}");

            if (response.IsSuccessStatusCode)
            {
                string content = await response.Content.ReadAsStringAsync();
                studentDto = JsonSerializer.Deserialize<StudentDto>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                if (studentDto != null)
                {
                    if (!string.IsNullOrEmpty(studentDto.ImageUrl))
                    {
                        try
                        {
                            byte[] imageBytes = Convert.FromBase64String(studentDto.ImageUrl);
                            PickedImageSource = ImageSource.FromStream(() => new MemoryStream(imageBytes));
                        }
                        catch (FormatException ex)
                        {
                            Console.WriteLine($"Error: Invalid base64 string: {ex.Message}");
                            await DisplayAlert("Error", "Invalid image data.", "OK");
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Error loading image: {ex.Message}");
                            await DisplayAlert("Error", "Error loading image.", "OK");
                        }
                    }
                    AddressList = new ObservableCollection<AddressDto>(studentDto.Addresses);

                    // Set the ListView's ItemsSource to the ObservableCollection
                    AddressesListView.ItemsSource = AddressList;
                    NameEntry.Text = studentDto.Name;
                    AdmissionDatePicker.Date = studentDto.AdmissionDate;
                    IsActiveCheckBox.IsChecked = studentDto.IsActive;
                }
                else
                {
                    await DisplayAlert("Error", "Failed to deserialize student data.", "OK");
                    await Navigation.PopAsync();
                }
            }
            else
            {
                await DisplayAlert("Error", "Failed to load student data.", "OK");
                await Navigation.PopAsync();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error during data load: {ex.Message}");
            await DisplayAlert("Error", "An error occurred while loading data.", "OK");
            await Navigation.PopAsync();
        }
    }
    private void OnRemoveAddressClicked(object sender, EventArgs e)
    {
        if (sender is Button button && button.CommandParameter is AddressDto address)
        {
            AddressList.Remove(address);
        }
    }
    private async void OnUploadImageClicked(object sender, EventArgs e)
    {
        try
        {
            var result = await FilePicker.PickAsync(new PickOptions { PickerTitle = "Please select a picture" });
            if (result != null)
            {
                using var stream = await result.OpenReadAsync();
                using var memoryStream = new MemoryStream();
                await stream.CopyToAsync(memoryStream);
                byte[] imageData = memoryStream.ToArray();
                studentDto.ImageBase64 = Convert.ToBase64String(imageData);
                PickedImageSource = ImageSource.FromFile(result.FullPath);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
    }

    private void OnAddAddressClicked(object sender, EventArgs e)
    {
        AddressList.Add(new AddressDto
        { Street = StreetEntry.Text, City = CityEntry.Text });
        StreetEntry.Text = "";
        CityEntry.Text = "";
    }

    private async void OnUpdateStudentClicked(object sender, EventArgs e)
    {
        if (studentDto == null)
        {
            await DisplayAlert("Error", "Student data not loaded.", "OK");
            return;
        }

        studentDto.Name = NameEntry.Text;
        studentDto.AdmissionDate = AdmissionDatePicker.Date;
        studentDto.IsActive = IsActiveCheckBox.IsChecked;
        studentDto.Addresses = AddressList.ToList(); // Update Addresses from the ObservableCollection
        studentDto.AddressJson = JsonSerializer.Serialize(AddressList);
        studentDto.ImageUrl = studentDto.ImageBase64;

        try
        {
            JsonSerializerOptions _serializerOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = true,
            };

            HttpClient _client = new HttpClient();
            _client.BaseAddress = new Uri(DeviceInfo.Platform == DevicePlatform.Android ? "http://10.0.2.2:5087" : "http://localhost:5087");

            string json = JsonSerializer.Serialize(studentDto, _serializerOptions);
            StringContent content = new StringContent(json, Encoding.UTF8, "application/json");
            _client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

            // Use PUT for updates, not POST
            HttpResponseMessage response = await _client.PutAsync($"/api/students/{studentDto.Id}", content); // Assuming studentDto has an Id

            if (response.IsSuccessStatusCode)
            {
                await DisplayAlert("Success", "Student updated successfully.", "OK");
                /*await Navigation.PopAsync();*/ // Navigate back or refresh
                await Navigation.PushAsync(new StudentListPage());
            }
            else
            {
                string errorContent = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"Update failed: {response.StatusCode}, {errorContent}");
                await DisplayAlert("Error", $"Failed to update student: {response.StatusCode}", "OK");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Update error: {ex.Message}");
            await DisplayAlert("Error", "An error occurred during update.", "OK");
        }
    }
}