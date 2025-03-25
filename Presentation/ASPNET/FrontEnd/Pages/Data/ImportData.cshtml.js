const App = {
    setup() {
        const state = Vue.reactive({
            isSubmitting: false,
        });

        const handleSubmit = async () => {
            state.isSubmitting = true;
            try {
                // Création de l'objet FormData pour envoyer les fichiers.
                const formData = new FormData();
                const fileInput1 = document.getElementById('file1');
                const fileInput2 = document.getElementById('file2');

                if (fileInput1.files.length > 0) {
                    formData.append('file1', fileInput1.files[0]);
                }
                if (fileInput2.files.length > 0) {
                    formData.append('file2', fileInput2.files[0]);
                }
                formData.append('userId',StorageManager.getUserId());

                // Vérifier qu'au moins un fichier a été sélectionné.
                if (!formData.has('file1') && !formData.has('file2')) {
                    Swal.fire({
                        icon: 'warning',
                        title: 'No files selected',
                        text: 'Please select at least one CSV file to upload.',
                        confirmButtonText: 'OK'
                    });
                    state.isSubmitting = false;
                    return;
                }

                const response = await AxiosManager.post('/Data/UploadCsv', formData, {
                    headers: {
                        'Content-Type': 'multipart/form-data'
                    }
                });

                if (response.data) {
                    if (response.data.content == null) {
                        Swal.fire({
                            icon: 'success',
                            title: 'Files uploaded successfully',
                            text: 'Processing files...',
                            timer: 2000,
                            showConfirmButton: false
                        });
                    } else {
                        Swal.fire({
                            icon: 'error',
                            title: 'Error importing csv',
                            text: response.data.content,
                            confirmButtonText: 'Try Again'
                        });                        
                    }
                } else {
                    Swal.fire({
                        icon: 'error',
                        title: 'Upload failed',
                        text: 'No response data received.',
                        confirmButtonText: 'Try Again'
                    });
                }
            } catch (error) {
                Swal.fire({
                    icon: 'error',
                    title: 'An error occurred',
                    text: error.response?.data?.message || 'Please try again.',
                    confirmButtonText: 'OK'
                });
            } finally {
                state.isSubmitting = false;
            }
        };

        return {
            state,
            handleSubmit
        };
    }
};

// Optionnel : Vérification de sécurité et masquage du spinner, si applicable.
Vue.onMounted(async () => {   
    try {
        await SecurityManager.authorizePage(['Data']);
        await SecurityManager.validateToken();
    } catch (error) {
        console.error(error);
    } finally {
        hideSpinnerAndShowContent();
    }
});

Vue.createApp(App).mount('#app');