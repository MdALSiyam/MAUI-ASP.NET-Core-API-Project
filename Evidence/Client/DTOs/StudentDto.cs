using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Client.DTOs
{
    public class StudentDto : INotifyPropertyChanged
    {
        private string name;
        private int id;
        private DateTime admissionDate;
        private bool isActive;
        private string imageBase64;
        private string imageUrl;
        private string addressJson;
        private List<AddressDto> addresses = new List<AddressDto> { new AddressDto() };

        public event PropertyChangedEventHandler? PropertyChanged;
        public void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        public int Id
        {
            get => id;
            set
            {
                id = value;
                NotifyPropertyChanged();
            }
        }
        public string ImageUrl
        {
            get => imageUrl;
            set
            {
                imageUrl = value;
                NotifyPropertyChanged();
            }
        }
        public List<AddressDto> Addresses
        {
            get => addresses;
            set
            {
                addresses = value;
                NotifyPropertyChanged();
            }
        }
        public string Name
        {
            get => name;
            set
            {
                name = value;
                NotifyPropertyChanged();
            }
        }
        public DateTime AdmissionDate
        {
            get => admissionDate;
            set
            {
                admissionDate = value;
                NotifyPropertyChanged();
            }
        }
        public bool IsActive
        {
            get => isActive;
            set
            {
                isActive = value;
                NotifyPropertyChanged();
            }
        }
        public string ImageBase64
        {
            get => imageBase64;
            set
            {
                imageBase64 = value;
                NotifyPropertyChanged();
            }
        }
        public string AddressJson
        {
            get => addressJson;
            set
            {
                addressJson = value;
                NotifyPropertyChanged();
            }
        }
    }
}
