'use client';

import { useState } from 'react';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { authApi, UserDto, CreateUserRequest, UpdateUserRequest } from '@/lib/api/auth-api';
import { useAuth } from '@/providers';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import { Label } from '@/components/ui/label';
import { Badge } from '@/components/ui/badge';
import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
  DialogFooter,
} from '@/components/ui/dialog';
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from '@/components/ui/select';
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from '@/components/ui/table';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';

type RoleFilter = 'All' | 'Admin' | 'Coach' | 'Athlete';

const ROLE_COLORS: Record<string, string> = {
  Admin: 'bg-purple-100 text-purple-800',
  Coach: 'bg-blue-100 text-blue-800',
  Athlete: 'bg-green-100 text-green-800',
};

export default function AdminUsersPage() {
  const { user: currentUser } = useAuth();
  const queryClient = useQueryClient();

  const [roleFilter, setRoleFilter] = useState<RoleFilter>('All');
  const [createOpen, setCreateOpen] = useState(false);
  const [editUser, setEditUser] = useState<UserDto | null>(null);
  const [resetUser, setResetUser] = useState<UserDto | null>(null);
  const [newPassword, setNewPassword] = useState('');
  const [formError, setFormError] = useState('');

  // Create user form state
  const [createForm, setCreateForm] = useState<CreateUserRequest>({
    email: '',
    username: '',
    password: '',
    role: 'Coach',
    coachId: '',
    athleteId: '',
  });

  // Edit user form state
  const [editForm, setEditForm] = useState<UpdateUserRequest & { id: string }>({
    id: '',
    username: '',
    email: '',
    isActive: true,
  });

  const { data: users = [], isLoading } = useQuery({
    queryKey: ['auth-users'],
    queryFn: () => authApi.getUsers(),
  });

  const createMutation = useMutation({
    mutationFn: (data: CreateUserRequest) => authApi.createUser(data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['auth-users'] });
      setCreateOpen(false);
      resetCreateForm();
    },
    onError: (err: unknown) => {
      const e = err as { response?: { data?: { message?: string } } };
      setFormError(e?.response?.data?.message ?? 'Failed to create user');
    },
  });

  const updateMutation = useMutation({
    mutationFn: ({ id, ...data }: UpdateUserRequest & { id: string }) =>
      authApi.updateUser(id, data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['auth-users'] });
      setEditUser(null);
    },
    onError: (err: unknown) => {
      const e = err as { response?: { data?: { message?: string } } };
      setFormError(e?.response?.data?.message ?? 'Failed to update user');
    },
  });

  const resetPasswordMutation = useMutation({
    mutationFn: ({ id, newPassword: pw }: { id: string; newPassword: string }) =>
      authApi.resetPassword(id, { newPassword: pw }),
    onSuccess: () => {
      setResetUser(null);
      setNewPassword('');
    },
    onError: (err: unknown) => {
      const e = err as { response?: { data?: { message?: string } } };
      setFormError(e?.response?.data?.message ?? 'Failed to reset password');
    },
  });

  const toggleActiveMutation = useMutation({
    mutationFn: (u: UserDto) => authApi.updateUser(u.id, { isActive: !u.isActive }),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ['auth-users'] }),
  });

  function resetCreateForm() {
    setCreateForm({ email: '', username: '', password: '', role: 'Coach', coachId: '', athleteId: '' });
    setFormError('');
  }

  function openEdit(u: UserDto) {
    setEditForm({ id: u.id, username: u.username, email: u.email, isActive: u.isActive });
    setFormError('');
    setEditUser(u);
  }

  const filtered = roleFilter === 'All' ? users : users.filter((u) => u.role === roleFilter);

  if (currentUser?.role !== 'Admin') {
    return (
      <div className="p-8 text-center text-slate-500">
        Access denied. Admin role required.
      </div>
    );
  }

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-2xl font-bold text-slate-900">User Management</h1>
          <p className="text-slate-500 text-sm mt-1">Create and manage platform users</p>
        </div>
        <Button onClick={() => { resetCreateForm(); setCreateOpen(true); }}>
          + New User
        </Button>
      </div>

      <Card>
        <CardHeader className="pb-3">
          <div className="flex items-center gap-3">
            <CardTitle className="text-base">Users</CardTitle>
            <div className="flex gap-1">
              {(['All', 'Admin', 'Coach', 'Athlete'] as RoleFilter[]).map((r) => (
                <button
                  key={r}
                  onClick={() => setRoleFilter(r)}
                  className={`px-3 py-1 rounded-full text-xs font-medium transition-colors ${
                    roleFilter === r
                      ? 'bg-slate-900 text-white'
                      : 'bg-slate-100 text-slate-600 hover:bg-slate-200'
                  }`}
                >
                  {r}
                </button>
              ))}
            </div>
          </div>
        </CardHeader>
        <CardContent>
          {isLoading ? (
            <p className="text-slate-500 text-sm py-4 text-center">Loading…</p>
          ) : (
            <Table>
              <TableHeader>
                <TableRow>
                  <TableHead>Username</TableHead>
                  <TableHead>Email</TableHead>
                  <TableHead>Role</TableHead>
                  <TableHead>Status</TableHead>
                  <TableHead className="text-right">Actions</TableHead>
                </TableRow>
              </TableHeader>
              <TableBody>
                {filtered.map((u) => (
                  <TableRow key={u.id}>
                    <TableCell className="font-medium">{u.username}</TableCell>
                    <TableCell className="text-slate-600">{u.email}</TableCell>
                    <TableCell>
                      <span className={`px-2 py-0.5 rounded text-xs font-medium ${ROLE_COLORS[u.role] ?? ''}`}>
                        {u.role}
                      </span>
                    </TableCell>
                    <TableCell>
                      <Badge variant={u.isActive ? 'default' : 'secondary'}>
                        {u.isActive ? 'Active' : 'Inactive'}
                      </Badge>
                    </TableCell>
                    <TableCell className="text-right">
                      <div className="flex justify-end gap-2">
                        <Button size="sm" variant="outline" onClick={() => openEdit(u)}>
                          Edit
                        </Button>
                        <Button
                          size="sm"
                          variant="outline"
                          onClick={() => { setResetUser(u); setNewPassword(''); setFormError(''); }}
                        >
                          Reset PW
                        </Button>
                        {u.id !== currentUser.id && (
                          <Button
                            size="sm"
                            variant={u.isActive ? 'destructive' : 'outline'}
                            onClick={() => toggleActiveMutation.mutate(u)}
                          >
                            {u.isActive ? 'Disable' : 'Enable'}
                          </Button>
                        )}
                      </div>
                    </TableCell>
                  </TableRow>
                ))}
                {filtered.length === 0 && (
                  <TableRow>
                    <TableCell colSpan={5} className="text-center text-slate-400 py-6">
                      No users found
                    </TableCell>
                  </TableRow>
                )}
              </TableBody>
            </Table>
          )}
        </CardContent>
      </Card>

      {/* Create User Dialog */}
      <Dialog open={createOpen} onOpenChange={(open) => { if (!open) resetCreateForm(); setCreateOpen(open); }}>
        <DialogContent className="max-w-md">
          <DialogHeader>
            <DialogTitle>Create User</DialogTitle>
          </DialogHeader>
          <form
            onSubmit={(e) => {
              e.preventDefault();
              setFormError('');
              const payload: CreateUserRequest = {
                email: createForm.email,
                username: createForm.username,
                password: createForm.password,
                role: createForm.role,
                athleteId: createForm.role === 'Athlete' && createForm.athleteId ? createForm.athleteId : undefined,
              };
              createMutation.mutate(payload);
            }}
            className="space-y-4"
          >
            <div className="space-y-2">
              <Label>Email</Label>
              <Input
                type="email"
                required
                value={createForm.email}
                onChange={(e) => setCreateForm((p) => ({ ...p, email: e.target.value }))}
              />
            </div>
            <div className="space-y-2">
              <Label>Username</Label>
              <Input
                required
                value={createForm.username}
                onChange={(e) => setCreateForm((p) => ({ ...p, username: e.target.value }))}
              />
            </div>
            <div className="space-y-2">
              <Label>Password</Label>
              <Input
                type="password"
                required
                minLength={8}
                value={createForm.password}
                onChange={(e) => setCreateForm((p) => ({ ...p, password: e.target.value }))}
              />
            </div>
            <div className="space-y-2">
              <Label>Role</Label>
              <Select
                value={createForm.role}
                onValueChange={(v) => setCreateForm((p) => ({ ...p, role: v as CreateUserRequest['role'] }))}
              >
                <SelectTrigger>
                  <SelectValue />
                </SelectTrigger>
                <SelectContent>
                  <SelectItem value="Coach">Coach</SelectItem>
                  <SelectItem value="Athlete">Athlete</SelectItem>
                  <SelectItem value="Admin">Admin</SelectItem>
                </SelectContent>
              </Select>
            </div>
            {createForm.role === 'Athlete' && (
              <div className="space-y-2">
                <Label>Athlete ID (UUID)</Label>
                <Input
                  required
                  placeholder="Paste athlete UUID from Athletes page"
                  value={createForm.athleteId ?? ''}
                  onChange={(e) => setCreateForm((p) => ({ ...p, athleteId: e.target.value }))}
                />
                <p className="text-xs text-slate-500">
                  The athlete must already exist (created via application approval).
                </p>
              </div>
            )}
            {formError && (
              <p className="text-sm text-red-600 bg-red-50 border border-red-200 rounded px-3 py-2">
                {formError}
              </p>
            )}
            <DialogFooter>
              <Button type="button" variant="outline" onClick={() => setCreateOpen(false)}>
                Cancel
              </Button>
              <Button type="submit" disabled={createMutation.isPending}>
                {createMutation.isPending ? 'Creating…' : 'Create'}
              </Button>
            </DialogFooter>
          </form>
        </DialogContent>
      </Dialog>

      {/* Edit User Dialog */}
      <Dialog open={!!editUser} onOpenChange={(open) => { if (!open) setEditUser(null); }}>
        <DialogContent className="max-w-md">
          <DialogHeader>
            <DialogTitle>Edit User — {editUser?.username}</DialogTitle>
          </DialogHeader>
          <form
            onSubmit={(e) => {
              e.preventDefault();
              setFormError('');
              updateMutation.mutate(editForm);
            }}
            className="space-y-4"
          >
            <div className="space-y-2">
              <Label>Email</Label>
              <Input
                type="email"
                required
                value={editForm.email ?? ''}
                onChange={(e) => setEditForm((p) => ({ ...p, email: e.target.value }))}
              />
            </div>
            <div className="space-y-2">
              <Label>Username</Label>
              <Input
                required
                value={editForm.username ?? ''}
                onChange={(e) => setEditForm((p) => ({ ...p, username: e.target.value }))}
              />
            </div>
            {formError && (
              <p className="text-sm text-red-600 bg-red-50 border border-red-200 rounded px-3 py-2">
                {formError}
              </p>
            )}
            <DialogFooter>
              <Button type="button" variant="outline" onClick={() => setEditUser(null)}>
                Cancel
              </Button>
              <Button type="submit" disabled={updateMutation.isPending}>
                {updateMutation.isPending ? 'Saving…' : 'Save'}
              </Button>
            </DialogFooter>
          </form>
        </DialogContent>
      </Dialog>

      {/* Reset Password Dialog */}
      <Dialog open={!!resetUser} onOpenChange={(open) => { if (!open) { setResetUser(null); setNewPassword(''); } }}>
        <DialogContent className="max-w-sm">
          <DialogHeader>
            <DialogTitle>Reset Password — {resetUser?.username}</DialogTitle>
          </DialogHeader>
          <form
            onSubmit={(e) => {
              e.preventDefault();
              setFormError('');
              if (resetUser) resetPasswordMutation.mutate({ id: resetUser.id, newPassword });
            }}
            className="space-y-4"
          >
            <div className="space-y-2">
              <Label>New Password</Label>
              <Input
                type="password"
                required
                minLength={8}
                value={newPassword}
                onChange={(e) => setNewPassword(e.target.value)}
              />
            </div>
            {formError && (
              <p className="text-sm text-red-600 bg-red-50 border border-red-200 rounded px-3 py-2">
                {formError}
              </p>
            )}
            <DialogFooter>
              <Button type="button" variant="outline" onClick={() => setResetUser(null)}>
                Cancel
              </Button>
              <Button type="submit" disabled={resetPasswordMutation.isPending}>
                {resetPasswordMutation.isPending ? 'Resetting…' : 'Reset'}
              </Button>
            </DialogFooter>
          </form>
        </DialogContent>
      </Dialog>
    </div>
  );
}
