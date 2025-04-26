using Client.DTOs;
using System.Collections.ObjectModel;
using System.Text;
using System.Text.Json;

namespace Client;

public partial class StudentCreatePage : ContentPage
{
    private StudentDto studentDto;
    private ImageSource pickedImageSource;
    public ObservableCollection<AddressDto> AddressList { get; set; } = new ObservableCollection<AddressDto>();
    public ImageSource PickedImageSource
    {
        get => this.pickedImageSource;
        set
        {
            this.pickedImageSource = value;
            OnPropertyChanged();
        }
    }
    public StudentCreatePage()
	{
		InitializeComponent();
        BindingContext = this;
        studentDto = new StudentDto();
    }
    private async void OnUploadImageClicked(object sender, EventArgs e)
    {
        try
        {
            var result = await FilePicker.PickAsync(new PickOptions
            {
                PickerTitle = "Please select a picture"
            });
            if (result != null)
            {
                using var stream = await result.OpenReadAsync();
                using var memoryStream = new MemoryStream();
                await stream.CopyToAsync(memoryStream);
                byte[] imagedata = memoryStream.ToArray();
                studentDto.ImageBase64 = Convert.ToBase64String(imagedata);
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
        AddressList.Add(new AddressDto { Street = AddressStreetEntry.Text, City = AddressCityEntry.Text });

        AddressStreetEntry.Text = "";
        AddressCityEntry.Text = "";
    }
    private async void OnAddStudentClicked(object sender, EventArgs e)
    {
        studentDto.Name = NameEntry.Text;
        studentDto.AdmissionDate = AdmissionDatePicker.Date;
        studentDto.IsActive = IsActiveCheckBox.IsChecked;
        studentDto.Addresses = AddressList.ToList();
        studentDto.AddressJson = JsonSerializer.Serialize(AddressList);
        studentDto.ImageUrl = studentDto.ImageBase64;
        try
        {
            JsonSerializerOptions _serializerOptions;
            _serializerOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = true,

            };
            HttpClient _client = new HttpClient();
            _client.BaseAddress = new Uri(DeviceInfo.Platform == DevicePlatform.Android ? "http://10.0.2.2:5087" : "http://localhost:5087");
            string json = System.Text.Json.JsonSerializer.Serialize<StudentDto>(studentDto, _serializerOptions);
            StringContent content = new StringContent(json, Encoding.UTF8, "application/json");
            _client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
            HttpResponseMessage response = await _client.PostAsync("/api/students", content);
            if (response.IsSuccessStatusCode)
            {
                await DisplayAlert("Success", "Student Created Successfully", "Ok");
                await Navigation.PushAsync(new StudentListPage());
            }
            else
            {
                await DisplayAlert("Error", "Failed to add Student", "Ok");
            }
        }
        catch (Exception ex)
        {

            Console.WriteLine(ex.Message);
        }
    }
}