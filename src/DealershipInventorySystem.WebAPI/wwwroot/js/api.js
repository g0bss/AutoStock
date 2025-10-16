// API Communication Layer for Dealership Inventory System
class DealershipAPI {
    constructor() {
        this.baseURL = window.location.origin;
        this.token = localStorage.getItem('authToken');
        this.currentUser = JSON.parse(localStorage.getItem('currentUser') || 'null');
    }

    // Helper method for making HTTP requests
    async request(endpoint, options = {}) {
        const url = `${this.baseURL}${endpoint}`;

        const config = {
            headers: {
                'Content-Type': 'application/json',
                ...options.headers,
            },
            ...options,
        };

        // Add auth token if available
        if (this.token) {
            config.headers['Authorization'] = `Bearer ${this.token}`;
        }

        try {
            const response = await fetch(url, config);

            // Check if response is ok
            if (!response.ok) {
                if (response.status === 401) {
                    this.logout();
                    throw new Error('Sessão expirada. Faça login novamente.');
                }

                let errorMessage = `Erro ${response.status}`;
                try {
                    const errorData = await response.json();
                    errorMessage = errorData.message || errorData.title || errorMessage;
                } catch {
                    errorMessage = response.statusText || errorMessage;
                }
                throw new Error(errorMessage);
            }

            // Return response data
            const contentType = response.headers.get('content-type');
            if (contentType && contentType.includes('application/json')) {
                return await response.json();
            }
            return await response.text();

        } catch (error) {
            console.error('API Request Error:', error);
            throw error;
        }
    }

    // Authentication
    async login(username, password) {
        const response = await this.request('/api/auth/login', {
            method: 'POST',
            body: JSON.stringify({ userName: username, password }),
        });

        this.token = response.token;
        this.currentUser = response.user; // API retorna 'user' minúsculo

        localStorage.setItem('authToken', this.token);
        localStorage.setItem('currentUser', JSON.stringify(this.currentUser));

        console.log('Login successful, full response:', response); // Debug completo
        console.log('User saved:', this.currentUser); // Debug

        return response;
    }

    logout() {
        this.token = null;
        this.currentUser = null;
        localStorage.removeItem('authToken');
        localStorage.removeItem('currentUser');
        window.location.href = '/login.html';
    }

    isAuthenticated() {
        return !!this.token;
    }

    getCurrentUser() {
        return this.currentUser;
    }

    // System Info
    async getHealth() {
        return await this.request('/api/info/health');
    }

    async getInfo() {
        return await this.request('/api/info/details');
    }

    async getStatus() {
        return await this.request('/api/info/status');
    }

    // Vehicles
    async getVehicles() {
        return await this.request('/api/vehicles');
    }

    async getVehicle(id) {
        return await this.request(`/api/vehicles/${id}`);
    }

    async createVehicle(vehicleData) {
        return await this.request('/api/vehicles', {
            method: 'POST',
            body: JSON.stringify(vehicleData),
        });
    }

    async updateVehicle(id, vehicleData) {
        return await this.request(`/api/vehicles/${id}`, {
            method: 'PUT',
            body: JSON.stringify(vehicleData),
        });
    }

    async deleteVehicle(id) {
        return await this.request(`/api/vehicles/${id}`, {
            method: 'DELETE',
        });
    }

    // Manufacturers
    async getManufacturers() {
        return await this.request('/api/manufacturers');
    }

    async getManufacturer(id) {
        return await this.request(`/api/manufacturers/${id}`);
    }

    async createManufacturer(manufacturerData) {
        return await this.request('/api/manufacturers', {
            method: 'POST',
            body: JSON.stringify(manufacturerData),
        });
    }

    async updateManufacturer(id, manufacturerData) {
        return await this.request(`/api/manufacturers/${id}`, {
            method: 'PUT',
            body: JSON.stringify(manufacturerData),
        });
    }

    async deleteManufacturer(id) {
        return await this.request(`/api/manufacturers/${id}`, {
            method: 'DELETE',
        });
    }

    // Customers
    async getCustomers() {
        return await this.request('/api/customers');
    }

    async getCustomer(id) {
        return await this.request(`/api/customers/${id}`);
    }

    async createCustomer(customerData) {
        return await this.request('/api/customers', {
            method: 'POST',
            body: JSON.stringify(customerData),
        });
    }

    async updateCustomer(id, customerData) {
        return await this.request(`/api/customers/${id}`, {
            method: 'PUT',
            body: JSON.stringify(customerData),
        });
    }

    async deleteCustomer(id) {
        return await this.request(`/api/customers/${id}`, {
            method: 'DELETE',
        });
    }

    // Vehicle Movements
    async getVehicleMovements() {
        return await this.request('/api/vehiclemovements');
    }

    async getVehicleMovement(id) {
        return await this.request(`/api/vehiclemovements/${id}`);
    }

    async createVehicleMovement(movementData) {
        return await this.request('/api/vehiclemovements', {
            method: 'POST',
            body: JSON.stringify(movementData),
        });
    }

    async getVehicleHistory(vehicleId) {
        return await this.request(`/api/vehiclemovements/vehicle/${vehicleId}/history`);
    }

    // Users
    async getUsers() {
        return await this.request('/api/users');
    }

    async getUser(id) {
        return await this.request(`/api/users/${id}`);
    }

    async createUser(userData) {
        return await this.request('/api/users', {
            method: 'POST',
            body: JSON.stringify(userData),
        });
    }

    async updateUser(id, userData) {
        return await this.request(`/api/users/${id}`, {
            method: 'PUT',
            body: JSON.stringify(userData),
        });
    }

    async deleteUser(id) {
        return await this.request(`/api/users/${id}`, {
            method: 'DELETE',
        });
    }
}

// Utility functions
const utils = {
    formatCurrency(value) {
        return new Intl.NumberFormat('pt-BR', {
            style: 'currency',
            currency: 'BRL'
        }).format(value);
    },

    formatDate(dateString) {
        if (!dateString) return '-';
        const date = new Date(dateString);
        return new Intl.DateTimeFormat('pt-BR', {
            day: '2-digit',
            month: '2-digit',
            year: 'numeric',
            hour: '2-digit',
            minute: '2-digit'
        }).format(date);
    },

    formatDateOnly(dateString) {
        if (!dateString) return '-';
        const date = new Date(dateString);
        return new Intl.DateTimeFormat('pt-BR', {
            day: '2-digit',
            month: '2-digit',
            year: 'numeric'
        }).format(date);
    },

    getVehicleStatusBadge(status) {
        const statusMap = {
            0: { text: 'Disponível', class: 'badge-success' },
            1: { text: 'Reservado', class: 'badge-warning' },
            2: { text: 'Vendido', class: 'badge-danger' },
            3: { text: 'Em Manutenção', class: 'badge-warning' },
            4: { text: 'Em Teste', class: 'badge-warning' }
        };
        return statusMap[status] || { text: 'Desconhecido', class: 'badge-secondary' };
    },

    getVehicleTypeBadge(type) {
        const typeMap = {
            0: { text: 'Novo', class: 'badge-success' },
            1: { text: 'Semi-novo', class: 'badge-warning' },
            2: { text: 'Usado', class: 'badge-secondary' }
        };
        return typeMap[type] || { text: 'Desconhecido', class: 'badge-secondary' };
    },

    getUserRoleBadge(role) {
        const roleMap = {
            0: { text: 'Administrador', class: 'badge-danger' },
            1: { text: 'Gerente', class: 'badge-warning' },
            2: { text: 'Vendedor', class: 'badge-success' },
            3: { text: 'Funcionário', class: 'badge-secondary' }
        };
        return roleMap[role] || { text: 'Desconhecido', class: 'badge-secondary' };
    },

    showAlert(message, type = 'success') {
        const alertDiv = document.createElement('div');
        alertDiv.className = `alert alert-${type}`;
        alertDiv.textContent = message;

        const container = document.querySelector('.main-content') || document.body;
        container.insertBefore(alertDiv, container.firstChild);

        setTimeout(() => {
            alertDiv.remove();
        }, 5000);
    },

    showError(message) {
        this.showAlert(message, 'error');
    },

    showSuccess(message) {
        this.showAlert(message, 'success');
    },

    showLoading(element) {
        if (element) {
            element.classList.add('loading');
        }
    },

    hideLoading(element) {
        if (element) {
            element.classList.remove('loading');
        }
    },

    validateVIN(vin) {
        // Basic VIN validation - 17 characters, alphanumeric except I, O, Q
        const vinRegex = /^[A-HJ-NPR-Z0-9]{17}$/;
        return vinRegex.test(vin);
    },

    validateEmail(email) {
        const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
        return emailRegex.test(email);
    },

    validateCPF(cpf) {
        // Basic CPF format validation
        const cpfRegex = /^\d{3}\.\d{3}\.\d{3}-\d{2}$/;
        return cpfRegex.test(cpf);
    },

    validatePhone(phone) {
        // Basic phone format validation
        const phoneRegex = /^\(\d{2}\)\s\d{4,5}-\d{4}$/;
        return phoneRegex.test(phone);
    }
};

// Modal management
const Modal = {
    show(modalId) {
        console.log('Showing modal:', modalId); // Debug log
        const modal = document.getElementById(modalId);
        if (modal) {
            modal.style.display = 'flex';
            modal.style.position = 'fixed';
            modal.style.top = '0';
            modal.style.left = '0';
            modal.style.width = '100%';
            modal.style.height = '100%';
            modal.style.zIndex = '9999';
            modal.style.alignItems = 'center';
            modal.style.justifyContent = 'center';
            modal.style.backgroundColor = 'rgba(0, 0, 0, 0.5)';
            document.body.style.overflow = 'hidden';
            console.log('Modal shown successfully'); // Debug log

            // Add click outside to close
            modal.addEventListener('click', (e) => {
                if (e.target === modal) {
                    Modal.hide(modalId);
                }
            });
        } else {
            console.error('Modal not found:', modalId); // Debug log
            // List all available modals for debugging
            const allModals = document.querySelectorAll('[id*="modal"]');
            console.log('Available modals:', Array.from(allModals).map(m => m.id));
        }
    },

    hide(modalId) {
        console.log('Hiding modal:', modalId); // Debug log
        const modal = document.getElementById(modalId);
        if (modal) {
            modal.style.display = 'none';
            document.body.style.overflow = 'auto';
        }
    },

    hideAll() {
        const modals = document.querySelectorAll('.modal-overlay, [id*="modal"]');
        modals.forEach(modal => {
            modal.style.display = 'none';
        });
        document.body.style.overflow = 'auto';
    }
};

// Global API instance
const api = new DealershipAPI();

// Auth guard for protected pages
function requireAuth() {
    if (!api.isAuthenticated()) {
        window.location.href = '/login.html';
        return false;
    }
    return true;
}

// Initialize navigation
function initNavigation() {
    // Set active nav item based on current page
    const currentPath = window.location.pathname;
    const navLinks = document.querySelectorAll('.sidebar-nav a');

    navLinks.forEach(link => {
        link.classList.remove('active');
        const href = link.getAttribute('href');
        if (href && (currentPath.includes(href) || (currentPath === '/' && href === '/dashboard.html'))) {
            link.classList.add('active');
        }
    });

    // Add logout functionality - using setTimeout to ensure DOM is ready
    setTimeout(() => {
        const logoutBtn = document.getElementById('logout-btn');
        console.log('InitNavigation - Looking for logout button:', logoutBtn); // Debug

        if (logoutBtn) {
            // DISABLED: Remove any existing listeners - this was breaking onclick handlers
            // logoutBtn.replaceWith(logoutBtn.cloneNode(true));
            // const newLogoutBtn = document.getElementById('logout-btn');

            // DISABLED: This event listener was conflicting with onclick handlers
            // console.log('InitNavigation - Adding click listener to logout button'); // Debug
            //
            // newLogoutBtn.addEventListener('click', (e) => {
            //     e.preventDefault();
            //     e.stopPropagation();
            //     console.log('Logout clicked - starting logout process'); // Debug log
            //
            //     // Confirm logout
            //     if (confirm('Tem certeza que deseja sair?')) {
            //         console.log('User confirmed logout, calling api.logout()'); // Debug
            //         api.logout();
            //     } else {
            //         console.log('User cancelled logout'); // Debug
            //     }
            // });

            // DISABLED: Also try to catch any links with href="#" and text "Sair"
            // document.querySelectorAll('a[href="#"]').forEach(link => {
            //     if (link.textContent.includes('Sair')) {
            //         console.log('Found Sair link, adding logout listener:', link);
            //         link.addEventListener('click', (e) => {
            //             e.preventDefault();
            //             if (confirm('Tem certeza que deseja sair?')) {
            //                 api.logout();
            //             }
            //         });
            //     }
            // });
        } else {
            // DISABLED: Logout button not found - searching for alternatives was conflicting
            // console.warn('Logout button not found - searching for alternatives');
            // // Try to find any element that might be the logout button
            // const allLinks = document.querySelectorAll('a');
            // allLinks.forEach(link => {
            //     if (link.textContent.includes('Sair')) {
            //         console.log('Found potential logout link:', link);
            //         link.addEventListener('click', (e) => {
            //             e.preventDefault();
            //             if (confirm('Tem certeza que deseja sair?')) {
            //                 api.logout();
            //             }
            //         });
            //     }
            // });
        }
    }, 200); // Aumentei o timeout para 200ms
}

// Initialize user info display
function initUserInfo() {
    setTimeout(() => {
        const currentUser = api.getCurrentUser();
        console.log('InitUserInfo - Current user:', currentUser); // Debug log
        console.log('InitUserInfo - LocalStorage currentUser:', localStorage.getItem('currentUser')); // Debug

        const userNameElement = document.querySelector('.user-name');
        const userAvatarElement = document.querySelector('.user-avatar');

        if (currentUser) {
            // Try to get name from different possible fields (case insensitive)
            let displayName = 'Usuário';
            let avatarLetter = 'U';

            // Try firstName field (camelCase or PascalCase)
            if (currentUser.firstName || currentUser.FirstName) {
                const firstName = currentUser.firstName || currentUser.FirstName;
                const lastName = currentUser.lastName || currentUser.LastName || '';
                displayName = `${firstName} ${lastName}`.trim();
                avatarLetter = firstName.charAt(0).toUpperCase();
            }
            // Try fullName field
            else if (currentUser.fullName || currentUser.FullName) {
                displayName = currentUser.fullName || currentUser.FullName;
                avatarLetter = displayName.charAt(0).toUpperCase();
            }
            // Try userName field
            else if (currentUser.userName || currentUser.UserName) {
                displayName = currentUser.userName || currentUser.UserName;
                avatarLetter = displayName.charAt(0).toUpperCase();
            }
            // Try email field
            else if (currentUser.email || currentUser.Email) {
                const email = currentUser.email || currentUser.Email;
                displayName = email.split('@')[0];
                avatarLetter = displayName.charAt(0).toUpperCase();
            }

            if (userNameElement) {
                userNameElement.textContent = displayName;
                console.log('Set user name to:', displayName); // Debug
            }

            if (userAvatarElement) {
                userAvatarElement.textContent = avatarLetter;
                console.log('Set avatar to:', avatarLetter); // Debug
            }
        } else {
            // Fallback - try to get from token or default
            const token = localStorage.getItem('authToken');
            if (token) {
                if (userNameElement) {
                    userNameElement.textContent = 'Administrador';
                }
                if (userAvatarElement) {
                    userAvatarElement.textContent = 'A';
                }
            } else {
                if (userNameElement) {
                    userNameElement.textContent = 'Usuário';
                }
                if (userAvatarElement) {
                    userAvatarElement.textContent = 'U';
                }
            }
            console.warn('User data not found in localStorage, using fallback');
        }
    }, 100);
}

// Export for use in other scripts
window.api = api;
window.utils = utils;
window.Modal = Modal;
window.requireAuth = requireAuth;
window.initNavigation = initNavigation;
window.initUserInfo = initUserInfo;