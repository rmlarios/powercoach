'use client';

import { useRef, useState } from 'react';
import { PageHeader } from '@/components/common';
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/components/ui/card';
import { Input } from '@/components/ui/input';
import { Button } from '@/components/ui/button';
import { Separator } from '@/components/ui/separator';
import { useCoach } from '@/providers';
import { Upload, Trash2, ImageIcon, Check, AlertTriangle } from 'lucide-react';
import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
  DialogDescription,
  DialogFooter,
} from '@/components/ui/dialog';
import { toast } from 'sonner';

const MAX_LOGO_SIZE = 512 * 1024; // 512KB
const ACCEPTED_TYPES = ['image/png', 'image/jpeg', 'image/webp', 'image/svg+xml'];

export default function SettingsPage() {
  const { coach, updateCoach } = useCoach();
  const fileInputRef = useRef<HTMLInputElement>(null);
  const [logoError, setLogoError] = useState<string | null>(null);
  const [profileSaved, setProfileSaved] = useState(false);
  const [nameValue, setNameValue] = useState(coach?.name || '');
  const [emailValue, setEmailValue] = useState(coach?.email || '');
  const [showDeleteConfirm, setShowDeleteConfirm] = useState(false);

  const handleLogoUpload = (e: React.ChangeEvent<HTMLInputElement>) => {
    setLogoError(null);
    const file = e.target.files?.[0];
    if (!file) return;

    if (!ACCEPTED_TYPES.includes(file.type)) {
      setLogoError('Formato no soportado. Usa PNG, JPG, WebP o SVG.');
      return;
    }

    if (file.size > MAX_LOGO_SIZE) {
      setLogoError('El archivo es muy grande. Máximo 512KB.');
      return;
    }

    const reader = new FileReader();
    reader.onload = () => {
      updateCoach({ logoUrl: reader.result as string });
    };
    reader.onerror = () => {
      setLogoError('Error al leer el archivo.');
    };
    reader.readAsDataURL(file);

    // Reset input so same file can be re-selected
    e.target.value = '';
  };

  const handleRemoveLogo = () => {
    updateCoach({ logoUrl: undefined });
    setLogoError(null);
  };

  const handleSaveProfile = () => {
    updateCoach({
      name: nameValue.trim() || coach?.name,
      email: emailValue.trim() || coach?.email,
    });
    setProfileSaved(true);
    setTimeout(() => setProfileSaved(false), 2000);
  };

  return (
    <div>
      <PageHeader
        title="Settings"
        description="Manage your account and preferences"
      />

      <div className="space-y-6 max-w-2xl">
        {/* Profile Settings */}
        <Card>
          <CardHeader>
            <CardTitle>Profile</CardTitle>
            <CardDescription>
              Update your personal information
            </CardDescription>
          </CardHeader>
          <CardContent className="space-y-4">
            <div className="grid gap-4 sm:grid-cols-2">
              <div className="space-y-2">
                <label className="text-sm font-medium text-slate-700">
                  Name
                </label>
                <Input
                  value={nameValue}
                  onChange={(e) => setNameValue(e.target.value)}
                />
              </div>
              <div className="space-y-2">
                <label className="text-sm font-medium text-slate-700">
                  Email
                </label>
                <Input
                  type="email"
                  value={emailValue}
                  onChange={(e) => setEmailValue(e.target.value)}
                />
              </div>
            </div>
            <Button onClick={handleSaveProfile}>
              {profileSaved ? (
                <>
                  <Check className="h-4 w-4 mr-2" />
                  Saved
                </>
              ) : (
                'Save Changes'
              )}
            </Button>
          </CardContent>
        </Card>

        <Separator />

        {/* Logo Settings */}
        <Card>
          <CardHeader>
            <CardTitle>Logo</CardTitle>
            <CardDescription>
              Upload your logo to include it in PDF exports
            </CardDescription>
          </CardHeader>
          <CardContent className="space-y-4">
            <div className="flex items-start gap-6">
              {/* Logo Preview */}
              <div className="flex-shrink-0">
                {coach?.logoUrl ? (
                  <div className="relative group">
                    <img
                      src={coach.logoUrl}
                      alt="Coach logo"
                      className="w-24 h-24 rounded-lg object-contain border border-slate-200 bg-white p-1"
                    />
                    <button
                      onClick={handleRemoveLogo}
                      className="absolute -top-2 -right-2 p-1 rounded-full bg-red-100 text-red-600 opacity-0 group-hover:opacity-100 transition-opacity hover:bg-red-200"
                      title="Eliminar logo"
                    >
                      <Trash2 className="h-3.5 w-3.5" />
                    </button>
                  </div>
                ) : (
                  <div className="w-24 h-24 rounded-lg border-2 border-dashed border-slate-300 flex items-center justify-center text-slate-400">
                    <ImageIcon className="h-8 w-8" />
                  </div>
                )}
              </div>

              {/* Upload Controls */}
              <div className="space-y-3 flex-1">
                <div>
                  <Button
                    variant="outline"
                    size="sm"
                    onClick={() => fileInputRef.current?.click()}
                  >
                    <Upload className="h-4 w-4 mr-2" />
                    {coach?.logoUrl ? 'Cambiar logo' : 'Subir logo'}
                  </Button>
                  <input
                    ref={fileInputRef}
                    type="file"
                    accept="image/png,image/jpeg,image/webp,image/svg+xml"
                    className="hidden"
                    onChange={handleLogoUpload}
                  />
                </div>
                <p className="text-xs text-slate-500">
                  PNG, JPG, WebP or SVG. Max 512KB.
                  {' '}The logo will appear in the header of exported PDFs.
                </p>
                {logoError && (
                  <p className="text-xs text-red-600">{logoError}</p>
                )}
              </div>
            </div>
          </CardContent>
        </Card>

        <Separator />

        {/* API Settings */}
        <Card>
          <CardHeader>
            <CardTitle>API Configuration</CardTitle>
            <CardDescription>
              Configure your API connection settings
            </CardDescription>
          </CardHeader>
          <CardContent className="space-y-4">
            <div className="space-y-2">
              <label className="text-sm font-medium text-slate-700">
                API Base URL
              </label>
              <Input 
                defaultValue={process.env.NEXT_PUBLIC_API_URL || 'http://localhost:5000/api'} 
                disabled 
              />
              <p className="text-xs text-slate-500">
                Configured via environment variable NEXT_PUBLIC_API_URL
              </p>
            </div>
          </CardContent>
        </Card>

        <Separator />

        {/* Danger Zone */}
        <Card className="border-red-200">
          <CardHeader>
            <CardTitle className="text-red-600">Danger Zone</CardTitle>
            <CardDescription>
              Irreversible actions. Be careful!
            </CardDescription>
          </CardHeader>
          <CardContent>
            <Button variant="destructive" onClick={() => setShowDeleteConfirm(true)}>
              <Trash2 className="h-4 w-4 mr-2" />
              Delete Account
            </Button>
          </CardContent>
        </Card>
      </div>

      {/* Delete Account Confirmation Dialog */}
      <Dialog open={showDeleteConfirm} onOpenChange={setShowDeleteConfirm}>
        <DialogContent>
          <DialogHeader>
            <DialogTitle className="flex items-center gap-2 text-red-600">
              <AlertTriangle className="h-5 w-5" />
              Delete Account
            </DialogTitle>
            <DialogDescription>
              This action is irreversible. All your data including athletes, programs, and training history will be permanently deleted.
            </DialogDescription>
          </DialogHeader>
          <DialogFooter>
            <Button variant="outline" onClick={() => setShowDeleteConfirm(false)}>Cancel</Button>
            <Button
              variant="destructive"
              onClick={() => {
                toast.info('Account deletion is not available yet');
                setShowDeleteConfirm(false);
              }}
            >
              Confirm Delete
            </Button>
          </DialogFooter>
        </DialogContent>
      </Dialog>
    </div>
  );
}
