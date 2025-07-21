# NewsSitePro 📰

A comprehensive news aggregation and social platform built with ASP.NET Core 6.0, featuring real-time news feeds, user interactions, and administrative capabilities.

## 🚀 Features

### Core Functionality
- **News Aggregation**: Integration with NewsAPI for real-time news fetching
- **User Management**: Complete user registration, authentication, and profile management
- **Social Features**: Like, save, share, and comment on news articles
- **Search**: Advanced search functionality for articles and users
- **Real-time Updates**: Background services for automatic content updates

### User Features
- 👤 **User Profiles**: Customizable profiles with bio, statistics, and activity tracking
- 📱 **Responsive Design**: Mobile-first design with Bootstrap 5
- 🔒 **JWT Authentication**: Secure token-based authentication system
- 📊 **Personal Dashboard**: View liked articles, saved content, and personal stats
- 💬 **Comments System**: Nested comments with like functionality
- 🏷️ **Category Filtering**: Browse news by categories (Technology, Sports, Politics, etc.)

### Administrative Features
- 🛡️ **Admin Dashboard**: Comprehensive admin panel with system statistics
- 👥 **User Management**: View, ban, unban, and manage user accounts
- 📈 **Analytics**: User activity logs and engagement metrics
- 🚨 **Content Moderation**: Report system for inappropriate content
- 📊 **Real-time Statistics**: Live dashboard with user and content metrics

## 🏗️ Architecture

This project follows the **3-Layer Architecture** pattern:

### 1. Data Access Layer (DAL)
- **`DBservices.cs`**: Central data access class with stored procedure integration
- **Stored Procedures**: Located in `/Database2025/` and `/StoredProcedures/`
- **Database Connection**: SQL Server with Entity Framework-style data access

### 2. Business Logic Layer (BL)
- **Models**: Data models and DTOs in `/BL/` directory
- **Services**: Business logic services implementing interfaces
  - `IUserService` & `UserService`: User-related operations
  - `INewsService` & `NewsService`: News article management
  - `ICommentService` & `CommentService`: Comment system
  - `IAdminService` & `AdminService`: Administrative functions
- **Validation**: Input validation and business rules enforcement

### 3. Presentation Layer
- **Controllers**: RESTful API controllers in `/Controllers/`
- **Razor Pages**: Server-side rendered pages in `/Pages/`
- **Client-Side**: JavaScript, CSS, and HTML assets in `/wwwroot/`

## 🛠️ Technology Stack

### Backend
- **Framework**: ASP.NET Core 6.0
- **Database**: SQL Server
- **Authentication**: JWT (JSON Web Tokens)
- **API Integration**: NewsAPI for external news content
- **Background Services**: Hosted services for scheduled tasks

### Frontend
- **UI Framework**: Bootstrap 5
- **JavaScript**: Vanilla JavaScript with modern ES6+ features
- **CSS**: Custom CSS with responsive design
- **Icons**: Font Awesome

### Development Tools
- **IDE**: Visual Studio / VS Code
- **Version Control**: Git & GitHub
- **Package Manager**: NuGet
- **API Testing**: Swagger/OpenAPI integration

## 📋 Prerequisites

- .NET 6.0 SDK or later
- SQL Server (LocalDB or full instance)
- Visual Studio 2022 or VS Code
- Git for version control

## 🚀 Getting Started

### 1. Clone the Repository
```bash
git clone https://github.com/ZeidanK/NewsSitePro.git
cd NewsSitePro
```

### 2. Database Setup
1. Update the connection string in `appsettings.json`:
```json
{
  "ConnectionStrings": {
    "myProjDB": "Server=(localdb)\\mssqllocaldb;Database=NewsSiteProDB;Trusted_Connection=true;MultipleActiveResultSets=true"
  }
}
```

2. Run the database scripts in order:
   - `/Database2025/database_setup.sql`
   - `/Database2025/tables.sql`
   - `/Database2025/stored_procedures.sql`
   - `/Database2025/constraints_indexes.sql`

### 3. News API Configuration
1. Get a free API key from [NewsAPI.org](https://newsapi.org/)
2. Add your API key to `appsettings.json`:
```json
{
  "NewsAPI": {
    "ApiKey": "your-api-key-here",
    "BaseUrl": "https://newsapi.org/v2/"
  }
}
```

### 4. Build and Run
```bash
dotnet restore
dotnet build
dotnet run
```

The application will be available at:
- HTTPS: `https://localhost:7128`
- HTTP: `http://localhost:5076`

## 📁 Project Structure

```
NewsSitePro/
├── 📁 BL/                          # Business Logic Layer
│   ├── 📁 Interfaces/              # Service interfaces
│   ├── 📁 Services/                # Service implementations
│   └── 📄 *.cs                     # Models and DTOs
├── 📁 Controllers/                 # API Controllers
├── 📁 DAL/                         # Data Access Layer
│   └── 📄 DBservices.cs            # Main data access class
├── 📁 Database2025/                # Database scripts
├── 📁 Pages/                       # Razor Pages
│   └── 📁 Shared/                  # Shared layouts and partials
├── 📁 Services/                    # External services
├── 📁 BackgroundServices/          # Background tasks
├── 📁 wwwroot/                     # Static assets
│   ├── 📁 css/                     # Stylesheets
│   ├── 📁 js/                      # JavaScript files
│   └── 📁 images/                  # Images
├── 📄 Program.cs                   # Application entry point
└── 📄 appsettings.json            # Configuration
```

## 🔧 Configuration

### Key Configuration Files
- **`appsettings.json`**: Main configuration including database and API settings
- **`Program.cs`**: Dependency injection and middleware configuration
- **`launchSettings.json`**: Development server settings

### Environment Variables
For production deployment, consider using environment variables for sensitive data:
- `NewsAPI__ApiKey`: NewsAPI key
- `ConnectionStrings__myProjDB`: Database connection string
- `JWT__SecretKey`: JWT signing key

## 🧪 Testing

### Manual Testing
1. Register a new user account
2. Browse and search news articles
3. Test social features (like, save, comment)
4. Access admin panel (admin users only)

### API Testing
Use the built-in Swagger UI at `/swagger` when running in development mode.

## 🚀 Deployment

### Prerequisites for Production
- Windows Server or Linux with .NET 6.0 runtime
- SQL Server instance
- IIS (Windows) or reverse proxy setup (Linux)

### Deployment Steps
1. Publish the application:
```bash
dotnet publish -c Release -o ./publish
```

2. Configure production database and update connection strings
3. Set up IIS application or configure reverse proxy
4. Ensure NewsAPI key is configured for production environment

## 🤝 Contributing

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/AmazingFeature`)
3. Commit your changes (`git commit -m 'Add some AmazingFeature'`)
4. Push to the branch (`git push origin feature/AmazingFeature`)
5. Open a Pull Request

## 📝 API Documentation

### Authentication Endpoints
- `POST /api/Auth/login` - User login
- `POST /api/Auth/register` - User registration

### News Endpoints
- `GET /api/Posts` - Get news articles with pagination
- `POST /api/Posts/Like/{id}` - Toggle article like
- `POST /api/Posts/Save/{id}` - Toggle article save
- `GET /api/Posts/Search` - Search articles

### User Endpoints
- `GET /api/User/{id}` - Get user profile
- `PUT /api/User/UpdateProfile` - Update user profile
- `GET /api/User/search` - Search users

### Admin Endpoints
- `GET /api/Admin/dashboard` - Admin dashboard stats
- `GET /api/Admin/users` - Manage users
- `GET /api/Admin/reports` - View reports

## 🔒 Security Features

- **JWT Authentication**: Secure token-based authentication
- **Input Validation**: Server-side validation for all inputs
- **SQL Injection Prevention**: Parameterized queries and stored procedures
- **XSS Protection**: Output encoding and Content Security Policy
- **CORS Configuration**: Controlled cross-origin resource sharing

## 📊 Database Schema

### Core Tables
- **Users_News**: User accounts and profiles
- **NewsArticles**: News articles and user posts
- **ArticleLikes**: Like relationships
- **SavedArticles**: Saved article relationships
- **Comments**: Article comments with threading support
- **Reports**: Content moderation reports

## 🎯 Future Enhancements

- [ ] **Real-time Notifications**: WebSocket integration for live updates
- [ ] **Email Notifications**: Newsletter and alert system
- [ ] **Advanced Analytics**: Detailed user engagement metrics
- [ ] **Mobile App**: React Native or Flutter mobile application
- [ ] **Content Recommendation**: AI-powered article recommendations
- [ ] **Multi-language Support**: Internationalization (i18n)

## 🐛 Known Issues

- Comments require page refresh after submission (enhancement planned)
- Profile picture upload size validation needed
- Search pagination could be improved


